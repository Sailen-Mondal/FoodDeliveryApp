// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Controller handling the public landing page, category showcase, and about/contact info.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using FoodDeliveryApp.Models.ViewModels;
using FoodDeliveryApp.Services;

namespace FoodDeliveryApp.Controllers
{
    /// <summary>
    /// Serves customer landing page and informational views.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IMenuService _menuService;

        /// <summary>
        /// Initializes a new instance with default menu service.
        /// </summary>
        public HomeController() : this(new MenuService())
        {
        }

        /// <summary>
        /// Initializes a new instance with injected menu service.
        /// </summary>
        public HomeController(IMenuService menuService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        }

        /// <summary>
        /// Renders the landing page with categories and bestseller dishes.
        /// </summary>
        public async Task<ActionResult> Index()
        {
            var categories = await _menuService.GetCategoriesAsync();
            var bestsellers = await _menuService.GetBestsellersAsync(8);

            var model = new HomeIndexViewModel
            {
                Categories = categories,
                Bestsellers = bestsellers
            };

            return View(model);
        }

        /// <summary>
        /// Renders the About Us page.
        /// </summary>
        public ActionResult About()
        {
            ViewBag.Message = "Foodly Kitchen is India's premier online culinary experience delivering authentic regional recipes fresh to your door.";
            return View();
        }

        /// <summary>
        /// Renders the Contact Us page.
        /// </summary>
        public ActionResult Contact()
        {
            ViewBag.Message = "We are always here to help. Reach our customer delight desk 24/7.";
            return View();
        }
    }
}