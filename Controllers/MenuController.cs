// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Controller providing the customer-facing food menu catalog, category browsing,
//                  dietary filters, price range filters, sorting, and recommended dishes.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Added parameterless constructor for DefaultControllerActivator.
// 2026-11-03   Sailen Mondal   Enhanced with sorting, price filtering, and recently viewed dishes.
// =========================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.Services;

namespace FoodDeliveryApp.Controllers
{
    /// <summary>
    /// Displays restaurant dishes and filters by category, dietary preferences, price, and search queries.
    /// </summary>
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;

        /// <summary>
        /// Initializes a new instance of MenuController with default menu service.
        /// </summary>
        public MenuController() : this(new MenuService())
        {
        }

        /// <summary>
        /// Initializes a new instance of MenuController with injected menu service.
        /// </summary>
        /// <param name="menuService">Menu service instance.</param>
        public MenuController(IMenuService menuService)
        {
            _menuService = menuService ?? new MenuService();
        }

        /// <summary>
        /// Displays the full menu catalog with interactive filters, sorting, and recommendation cards.
        /// </summary>
        /// <param name="categoryId">Optional category filter.</param>
        /// <param name="isVeg">Optional vegetarian filter.</param>
        /// <param name="search">Optional text search term.</param>
        /// <param name="sortBy">Optional sort mode: popular, price_asc, price_desc, rating.</param>
        /// <param name="minPrice">Optional minimum price filter.</param>
        /// <param name="maxPrice">Optional maximum price filter.</param>
        /// <returns>Rendered menu catalog view.</returns>
        [HttpGet]
        public async Task<ActionResult> Index(int? categoryId, bool? isVeg, string search, string sortBy = "popular", decimal? minPrice = null, decimal? maxPrice = null)
        {
            var categories = await _menuService.GetCategoriesAsync();
            var items = await _menuService.GetMenuItemsAsync(categoryId, isVeg, search);

            // Apply price range filtering if supplied
            if (minPrice.HasValue)
            {
                items = items.Where(i => i.Price >= minPrice.Value).ToList();
            }
            if (maxPrice.HasValue)
            {
                items = items.Where(i => i.Price <= maxPrice.Value).ToList();
            }

            // Apply client-friendly sorting
            switch ((sortBy ?? "popular").ToLowerInvariant())
            {
                case "price_asc":
                    items = items.OrderBy(i => i.Price).ToList();
                    break;
                case "price_desc":
                    items = items.OrderByDescending(i => i.Price).ToList();
                    break;
                case "rating":
                    items = items.OrderByDescending(i => i.Rating).ThenBy(i => i.ItemId).ToList();
                    break;
                case "popular":
                default:
                    items = items.OrderByDescending(i => i.Rating).ThenByDescending(i => i.Price).ToList();
                    break;
            }

            // Fetch 4 featured / recently viewed recommendations for the bottom carousel
            var recommendations = await _menuService.GetBestsellersAsync(4);

            ViewBag.Categories = categories;
            ViewBag.ActiveCategoryId = categoryId;
            ViewBag.IsVeg = isVeg;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy ?? "popular";
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Recommendations = recommendations;

            return View(items);
        }
    }
}
