// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Repository interface contract for MenuItems and Category catalog operations.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Data.Repositories
{
    /// <summary>
    /// Contract for Menu and Category catalog data access.
    /// </summary>
    public interface IMenuItemRepository
    {
        /// <summary>
        /// Retrieves all active menu categories.
        /// </summary>
        /// <returns>List of Category entities.</returns>
        Task<List<Category>> GetCategoriesAsync();

        /// <summary>
        /// Retrieves customer-facing available menu items matching optional filters.
        /// </summary>
        /// <param name="categoryId">Optional category filter.</param>
        /// <param name="isVeg">Optional vegetarian flag filter.</param>
        /// <param name="searchTerm">Optional dish name/description search term.</param>
        /// <returns>List of matching MenuItem entities.</returns>
        Task<List<MenuItem>> GetAllAsync(int? categoryId = null, bool? isVeg = null, string searchTerm = null);

        /// <summary>
        /// Retrieves all menu items for the administrative catalog dashboard regardless of availability.
        /// </summary>
        /// <returns>List of all MenuItem entities.</returns>
        Task<List<MenuItem>> GetAdminAllAsync();

        /// <summary>
        /// Retrieves a single menu item by ID.
        /// </summary>
        /// <param name="itemId">Item ID.</param>
        /// <returns>Matching MenuItem or null.</returns>
        Task<MenuItem> GetByIdAsync(int itemId);

        /// <summary>
        /// Toggles the IsAvailable flag for a menu item.
        /// </summary>
        /// <param name="itemId">Item ID.</param>
        /// <returns>The updated availability state.</returns>
        Task<bool> ToggleAvailabilityAsync(int itemId);

        /// <summary>
        /// Updates the price for a menu item.
        /// </summary>
        /// <param name="itemId">Item ID.</param>
        /// <param name="newPrice">New price in Indian Rupees (₹).</param>
        /// <returns>Number of rows affected.</returns>
        Task<int> UpdatePriceAsync(int itemId, decimal newPrice);
    }
}
