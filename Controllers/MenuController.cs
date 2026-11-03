// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Controller providing the customer-facing food menu catalog, category browsing,
//                  and veg/non-veg filter capabilities.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Added parameterless constructor for DefaultControllerActivator.
// =========================================================================================

using System.Threading.Tasks;
using System.Web.Mvc;
using FoodDeliveryApp.Services;

namespace FoodDeliveryApp.Controllers
{
    /// <summary>
    /// Displays restaurant dishes and filters by category, dietary preferences, and search queries.
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

        // GET: /Menu
        [HttpGet]
        public async Task<ActionResult> Index(int? categoryId, bool? isVeg, string search)
        {
            var categories = await _menuService.GetCategoriesAsync();
            var items = await _menuService.GetMenuItemsAsync(categoryId, isVeg, search);

            ViewBag.Categories = categories;
            ViewBag.ActiveCategoryId = categoryId;
            ViewBag.IsVeg = isVeg;
            ViewBag.SearchTerm = search;

            return View(items);
        }
    }
}
