// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model representing a food item inside the customer's shopping cart session.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;

namespace FoodDeliveryApp.Models.ViewModels
{
    /// <summary>
    /// Represents an item stored in the customer's interactive shopping cart.
    /// </summary>
    [Serializable]
    public class CartItemViewModel
    {
        /// <summary>
        /// Menu item unique identifier.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Name of the dish.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Unit price in Indian Rupees (₹).
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Selected quantity.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Thumbnail food image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Dietary indicator: true for Veg, false for Non-Veg.
        /// </summary>
        public bool IsVeg { get; set; }

        /// <summary>
        /// Total price for this line item in Indian Rupees (₹).
        /// </summary>
        public decimal TotalPrice => Price * Quantity;
    }
}
