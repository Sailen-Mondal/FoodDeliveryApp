// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Controller orchestrating customer order checkout, placement via Stored Procedures,
//                  and personal order history viewing.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Added parameterless constructor for DefaultControllerActivator.
// 2026-11-03   Sailen Mondal   Upgraded Checkout and Confirmation with payment details and idempotency token.
// =========================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FoodDeliveryApp.Data.Repositories;
using FoodDeliveryApp.Infrastructure.Filters;
using FoodDeliveryApp.Models.ViewModels;
using FoodDeliveryApp.Services;

namespace FoodDeliveryApp.Controllers
{
    /// <summary>
    /// Handles customer checkout flows, atomic order submission, and order history tracking.
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        private const string CartSessionKey = "Foodly_Cart";
        private readonly IOrderService _orderService;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of OrderController with default services.
        /// </summary>
        public OrderController() : this(new OrderService(), new UserRepository())
        {
        }

        /// <summary>
        /// Initializes a new instance of OrderController with injected services.
        /// </summary>
        /// <param name="orderService">Order service instance.</param>
        /// <param name="userRepository">User repository instance.</param>
        public OrderController(IOrderService orderService, IUserRepository userRepository)
        {
            _orderService = orderService ?? new OrderService();
            _userRepository = userRepository ?? new UserRepository();
        }

        private List<CartItemViewModel> GetCart()
        {
            return Session[CartSessionKey] as List<CartItemViewModel> ?? new List<CartItemViewModel>();
        }

        // GET: /Order/Checkout
        [HttpGet]
        public async Task<ActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is currently empty. Please choose delicious dishes to order!";
                return RedirectToAction("Index", "Menu");
            }

            var user = await _userRepository.GetByEmailAsync(User.Identity.Name);

            var model = new OrderPlacementViewModel
            {
                DeliveryAddress = user?.Address ?? string.Empty,
                ContactPhone = user?.PhoneNumber ?? string.Empty,
                PaymentMethod = "UPI",
                UpiApp = "GPay",
                SelectedBank = "HDFC Bank",
                IdempotencyToken = Guid.NewGuid().ToString("N"),
                Items = cart
            };

            return View(model);
        }

        // POST: /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RateLimit(Seconds = 60, MaxRequests = 10, Message = "Order request limit reached. Please wait a moment before trying again.")]
        public async Task<ActionResult> PlaceOrder(OrderPlacementViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items before placing an order.";
                return RedirectToAction("Index", "Menu");
            }

            model.Items = cart;

            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            var user = await _userRepository.GetByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to locate your customer profile. Please sign in again.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                int orderId = await _orderService.PlaceOrderAsync(user.UserId, model);
                if (orderId > 0)
                {
                    // Clear the customer's cart upon successful order placement
                    Session[CartSessionKey] = new List<CartItemViewModel>();
                    TempData["SuccessMessage"] = $"Order #{orderId} placed successfully! The kitchen is preparing your dishes.";
                    return RedirectToAction("Confirmation", new { id = orderId });
                }

                ModelState.AddModelError(string.Empty, "Unable to place your order. Please review your cart and try again.");
                return View("Checkout", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating your order: " + ex.Message);
                return View("Checkout", model);
            }
        }

        // GET: /Order/Confirmation/5
        [HttpGet]
        public async Task<ActionResult> Confirmation(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("History");
            }

            var user = await _userRepository.GetByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Anti-IDOR Authorization Check: User must own the order or be an Administrator
            bool isAdmin = User.IsInRole("Admin");
            var order = await _orderService.GetOrderByIdForUserAsync(id, user.UserId, isAdmin);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found or you are not authorized to access this order.";
                return RedirectToAction("History");
            }

            ViewBag.OrderId = order.OrderId;
            return View(order);
        }

        // GET: /Order/History
        [HttpGet]
        public async Task<ActionResult> History()
        {
            var user = await _userRepository.GetByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetCustomerOrderHistoryAsync(user.UserId);
            return View(orders);
        }
    }
}
