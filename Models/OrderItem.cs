// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Domain entity representing an itemized line inside a customer order.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

namespace FoodDeliveryApp.Models
{
    /// <summary>
    /// Represents an individual itemized line record in an order.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Unique primary key identifier for the order line item.
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// Foreign key referencing the parent Order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Foreign key referencing the original MenuItem.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Snapshot of item name at time of order.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Snapshot of unit price at time of order in Indian Rupees (₹).
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Quantity of this item ordered.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Total calculated line price (UnitPrice * Quantity).
        /// </summary>
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
