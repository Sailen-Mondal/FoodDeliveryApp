// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Service implementation orchestrating Menu and Category business logic.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDeliveryApp.Data.Repositories;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Services
{
    /// <summary>
    /// Implements menu catalog business workflows and validation.
    /// </summary>
    public class MenuService : IMenuService
    {
        private readonly IMenuItemRepository _menuItemRepository;

        /// <summary>
        /// Initializes a new instance of MenuService with the specified repository.
        /// </summary>
        /// <param name="menuItemRepository">Repository for menu persistence.</param>
        public MenuService(IMenuItemRepository menuItemRepository = null)
        {
            _menuItemRepository = menuItemRepository ?? new MenuItemRepository();
        }

        /// <summary>
        /// Retrieves active categories.
        /// </summary>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _menuItemRepository.GetCategoriesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves customer-facing menu items.
        /// </summary>
        public async Task<List<MenuItem>> GetMenuItemsAsync(int? categoryId = null, bool? isVeg = null, string searchTerm = null)
        {
            return await _menuItemRepository.GetAllAsync(categoryId, isVeg, searchTerm).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves top rated dishes to feature as bestsellers.
        /// </summary>
        public async Task<List<MenuItem>> GetBestsellersAsync(int count = 8)
        {
            var items = await _menuItemRepository.GetAllAsync().ConfigureAwait(false);
            return items.OrderByDescending(i => i.Rating).ThenBy(i => i.ItemId).Take(count).ToList();
        }

        /// <summary>
        /// Retrieves all menu items for admin dashboard.
        /// </summary>
        public async Task<List<MenuItem>> GetAdminMenuItemsAsync()
        {
            return await _menuItemRepository.GetAdminAllAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves single menu item by ID.
        /// </summary>
        public async Task<MenuItem> GetMenuItemByIdAsync(int itemId)
        {
            return await _menuItemRepository.GetByIdAsync(itemId).ConfigureAwait(false);
        }

        /// <summary>
        /// Toggles item availability.
        /// </summary>
        public async Task<bool> ToggleAvailabilityAsync(int itemId)
        {
            return await _menuItemRepository.ToggleAvailabilityAsync(itemId).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates item price.
        /// </summary>
        public async Task<bool> UpdatePriceAsync(int itemId, decimal newPrice)
        {
            if (newPrice <= 0)
            {
                return false;
            }
            int rows = await _menuItemRepository.UpdatePriceAsync(itemId, newPrice).ConfigureAwait(false);
            return rows > 0;
        }
    }
}
