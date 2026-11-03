// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model for customer past order history items.
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
    /// Displays a historical order record in the customer's order history dashboard.
    /// </summary>
    public class OrderHistoryItemViewModel
    {
        /// <summary>
        /// Unique order identifier.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Delivery destination snapshot.
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Contact phone number snapshot.
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Payment method snapshot.
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Total order payable amount in Indian Rupees (₹).
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Order lifecycle status: 'Pending', 'Accepted', or 'Rejected'.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Timestamp when order was placed.
        /// </summary>
        public DateTime PlacedAt { get; set; }

        /// <summary>
        /// Timestamp when order status was updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Aggregated item summary string.
        /// </summary>
        public string ItemsSummary { get; set; }
    }
}
