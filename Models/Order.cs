// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Domain entity representing a placed customer order with snapshot details.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Collections.Generic;

namespace FoodDeliveryApp.Models
{
    /// <summary>
    /// Represents a customer order record in the system.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unique primary key identifier for the order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Foreign key referencing the customer user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of customer at time of order.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Customer email address.
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Snapshot of delivery address for this specific order.
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Snapshot of customer contact phone number.
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Payment method selected (e.g. Cash on Delivery, UPI on Delivery).
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Total order payable amount in Indian Rupees (₹).
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Current lifecycle status: 'Pending', 'Accepted', or 'Rejected'.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Timestamp when order was placed.
        /// </summary>
        public DateTime PlacedAt { get; set; }

        /// <summary>
        /// Timestamp when status was updated by admin or auto-rejection rule.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Comma-separated summary string of ordered dishes and quantities.
        /// </summary>
        public string ItemsSummary { get; set; }

        /// <summary>
        /// Collection of individual line items associated with this order.
        /// </summary>
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
