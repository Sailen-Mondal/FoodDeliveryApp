// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Domain entity representing a food menu category.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

namespace FoodDeliveryApp.Models
{
    /// <summary>
    /// Represents a category for classifying menu items.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unique primary key identifier for the category.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Name of the category (e.g., Biryani & Rice Bowls, Pizzas & Breads).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display sequence order in navigation tabs and catalogs.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Flag indicating whether the category is currently active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
