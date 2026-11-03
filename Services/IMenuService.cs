// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Service interface contract for Menu and Category business operations.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Services
{
    /// <summary>
    /// Service contract for menu catalog queries, filtering, and inventory updates.
    /// </summary>
    public interface IMenuService
    {
        /// <summary>
        /// Retrieves all active menu categories.
        /// </summary>
        /// <returns>List of Category entities.</returns>
        Task<List<Category>> GetCategoriesAsync();

        /// <summary>
        /// Retrieves customer-facing menu items matching optional category, dietary flag, and search query.
        /// </summary>
        /// <param name="categoryId">Optional category filter.</param>
        /// <param name="isVeg">Optional vegetarian flag filter.</param>
        /// <param name="searchTerm">Optional text search keyword.</param>
        /// <returns>List of active MenuItem entities.</returns>
        Task<List<MenuItem>> GetMenuItemsAsync(int? categoryId = null, bool? isVeg = null, string searchTerm = null);

        /// <summary>
        /// Retrieves top rated dishes to feature as bestsellers on the landing page.
        /// </summary>
        /// <param name="count">Maximum count of items to return.</param>
        /// <returns>List of top rated MenuItem entities.</returns>
        Task<List<MenuItem>> GetBestsellersAsync(int count = 8);

        /// <summary>
        /// Retrieves all menu items including out-of-stock items for the administrator portal.
        /// </summary>
        /// <returns>List of all MenuItem entities.</returns>
        Task<List<MenuItem>> GetAdminMenuItemsAsync();

        /// <summary>
        /// Retrieves a single menu item by ID.
        /// </summary>
        /// <param name="itemId">Item ID.</param>
        /// <returns>Matching MenuItem or null.</returns>
        Task<MenuItem> GetMenuItemByIdAsync(int itemId);

        /// <summary>
        /// Toggles item availability state in the catalog.
        /// </summary>
        /// <param name="itemId">Item ID.</param>
        /// <returns>The updated availability state.</returns>
        Task<bool> ToggleAvailabilityAsync(int itemId);

        /// <summary>
        /// Updates the price for a menu item.
        /// </summary>
        /// <param name="itemId">Item ID.</param>
        /// <param name="newPrice">New price in Indian Rupees (₹).</param>
        /// <returns>True if successfully updated.</returns>
        Task<bool> UpdatePriceAsync(int itemId, decimal newPrice);
    }
}
