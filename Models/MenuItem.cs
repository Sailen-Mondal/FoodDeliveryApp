// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Domain entity representing an individual food menu item.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;

namespace FoodDeliveryApp.Models
{
    /// <summary>
    /// Represents an individual dish or item available on the menu.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Unique primary key identifier for the menu item.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Foreign key referencing the parent Category.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Display name of the parent category.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Dish name (e.g., Hyderabadi Chicken Dum Biryani, Classic Margherita Pizza).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of ingredients and preparation style.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Item price in Indian Rupees (₹).
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// URL linking to the food photography image.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Dietary indicator: true for Pure Vegetarian, false for Non-Vegetarian.
        /// </summary>
        public bool IsVeg { get; set; }

        /// <summary>
        /// Availability flag controlled by kitchen inventory and admin.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Customer rating on a 5-star scale (e.g. 4.8).
        /// </summary>
        public decimal Rating { get; set; }

        /// <summary>
        /// Estimated preparation time in minutes.
        /// </summary>
        public int PrepTimeMinutes { get; set; }

        /// <summary>
        /// Date and time when the item was added to the catalog.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
