// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Controller managing session-backed customer shopping cart operations via AJAX.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Added parameterless constructor for DefaultControllerActivator.
// =========================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FoodDeliveryApp.Models.ViewModels;
using FoodDeliveryApp.Services;

namespace FoodDeliveryApp.Controllers
{
    /// <summary>
    /// Handles asynchronous cart interactions, item additions, quantity modifications, and summary queries.
    /// </summary>
    public class CartController : Controller
    {
        private const string CartSessionKey = "Foodly_Cart";
        private readonly IMenuService _menuService;

        /// <summary>
        /// Initializes a new instance of CartController with default menu service.
        /// </summary>
        public CartController() : this(new MenuService())
        {
        }

        /// <summary>
        /// Initializes a new instance of CartController with injected menu service.
        /// </summary>
        /// <param name="menuService">Menu service instance.</param>
        public CartController(IMenuService menuService)
        {
            _menuService = menuService ?? new MenuService();
        }

        private List<CartItemViewModel> GetCart()
        {
            var cart = Session[CartSessionKey] as List<CartItemViewModel>;
            if (cart == null)
            {
                cart = new List<CartItemViewModel>();
                Session[CartSessionKey] = cart;
            }
            return cart;
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            Session[CartSessionKey] = cart;
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<JsonResult> Add(int itemId, int quantity = 1)
        {
            if (itemId <= 0 || quantity <= 0)
            {
                return Json(new { success = false, message = "Invalid item or quantity." });
            }

            var menuItem = await _menuService.GetMenuItemByIdAsync(itemId);
            if (menuItem == null || !menuItem.IsAvailable)
            {
                return Json(new { success = false, message = "This item is currently unavailable." });
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ItemId = menuItem.ItemId,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = quantity,
                    ImageUrl = menuItem.ImageUrl,
                    IsVeg = menuItem.IsVeg
                });
            }

            SaveCart(cart);

            decimal subTotal = cart.Sum(i => i.TotalPrice);
            decimal deliveryFee = subTotal > 500m || subTotal == 0m ? 0m : 40m;
            decimal total = subTotal + deliveryFee;

            return Json(new
            {
                success = true,
                message = $"{menuItem.Name} added to cart!",
                itemCount = cart.Sum(i => i.Quantity),
                subTotal = subTotal,
                deliveryFee = deliveryFee,
                total = total,
                items = cart
            });
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public JsonResult UpdateQuantity(int itemId, int delta)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found in cart." });
            }

            item.Quantity += delta;
            if (item.Quantity <= 0)
            {
                cart.Remove(item);
            }

            SaveCart(cart);

            decimal subTotal = cart.Sum(i => i.TotalPrice);
            decimal deliveryFee = subTotal > 500m || subTotal == 0m ? 0m : 40m;
            decimal total = subTotal + deliveryFee;

            return Json(new
            {
                success = true,
                itemCount = cart.Sum(i => i.Quantity),
                subTotal = subTotal,
                deliveryFee = deliveryFee,
                total = total,
                items = cart
            });
        }

        // GET: /Cart/GetSummary
        [HttpGet]
        public JsonResult GetSummary()
        {
            var cart = GetCart();
            decimal subTotal = cart.Sum(i => i.TotalPrice);
            decimal deliveryFee = subTotal > 500m || subTotal == 0m ? 0m : 40m;
            decimal total = subTotal + deliveryFee;

            return Json(new
            {
                success = true,
                itemCount = cart.Sum(i => i.Quantity),
                subTotal = subTotal,
                deliveryFee = deliveryFee,
                total = total,
                items = cart
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: /Cart/Clear
        [HttpPost]
        public JsonResult Clear()
        {
            Session[CartSessionKey] = new List<CartItemViewModel>();
            return Json(new { success = true, itemCount = 0, subTotal = 0m, total = 0m });
        }
    }
}
