// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Controller providing administrative order management, 60-second live status dashboard,
//                  and restaurant menu catalog/price administration.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Added parameterless constructor for DefaultControllerActivator.
// =========================================================================================

using System.Threading.Tasks;
using System.Web.Mvc;
using FoodDeliveryApp.Models.ViewModels;
using FoodDeliveryApp.Services;

namespace FoodDeliveryApp.Controllers
{
    /// <summary>
    /// Administrative controller restricted strictly to users with the 'Admin' role.
    /// Handles order status transitions and product catalog controls.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMenuService _menuService;

        /// <summary>
        /// Initializes a new instance of AdminController with default services.
        /// </summary>
        public AdminController() : this(new OrderService(), new MenuService())
        {
        }

        /// <summary>
        /// Initializes a new instance of AdminController with injected services.
        /// </summary>
        /// <param name="orderService">Order service instance.</param>
        /// <param name="menuService">Menu service instance.</param>
        public AdminController(IOrderService orderService, IMenuService menuService)
        {
            _orderService = orderService ?? new OrderService();
            _menuService = menuService ?? new MenuService();
        }

        // GET: /Admin/Orders?tab=pending
        [HttpGet]
        public async Task<ActionResult> Orders(string tab = "pending")
        {
            var viewModel = await _orderService.GetAdminOrdersDashboardAsync(tab);
            return View(viewModel);
        }

        // POST: /Admin/AcceptOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AcceptOrder(int orderId, string returnTab = "pending")
        {
            bool success = await _orderService.UpdateOrderStatusAsync(orderId, "Accepted");
            if (success)
            {
                TempData["SuccessMessage"] = $"Order #{orderId} has been successfully accepted!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Order #{orderId} could not be accepted. It may have already been processed or automatically rejected due to the 5-minute timeout.";
            }

            return RedirectToAction("Orders", new { tab = returnTab });
        }

        // POST: /Admin/RejectOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RejectOrder(int orderId, string returnTab = "pending")
        {
            bool success = await _orderService.UpdateOrderStatusAsync(orderId, "Rejected");
            if (success)
            {
                TempData["SuccessMessage"] = $"Order #{orderId} has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Order #{orderId} could not be updated. It may have already been processed.";
            }

            return RedirectToAction("Orders", new { tab = returnTab });
        }

        // GET: /Admin/MenuItems
        [HttpGet]
        public async Task<ActionResult> MenuItems()
        {
            var items = await _menuService.GetAdminMenuItemsAsync();
            var categories = await _menuService.GetCategoriesAsync();

            var viewModel = new AdminMenuItemViewModel
            {
                MenuItems = items,
                Categories = categories
            };

            return View(viewModel);
        }

        // POST: /Admin/ToggleAvailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleAvailability(int itemId)
        {
            bool newState = await _menuService.ToggleAvailabilityAsync(itemId);
            TempData["SuccessMessage"] = $"Item status updated to {(newState ? "Available" : "Out of Stock")}.";
            return RedirectToAction("MenuItems");
        }

        // POST: /Admin/UpdatePrice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePrice(int itemId, decimal newPrice)
        {
            if (newPrice <= 0)
            {
                TempData["ErrorMessage"] = "Price must be greater than zero.";
                return RedirectToAction("MenuItems");
            }

            bool success = await _menuService.UpdatePriceAsync(itemId, newPrice);
            if (success)
            {
                TempData["SuccessMessage"] = $"Price updated successfully to \u20B9{newPrice:N2}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update price. Please try again.";
            }

            return RedirectToAction("MenuItems");
        }
    }
}
