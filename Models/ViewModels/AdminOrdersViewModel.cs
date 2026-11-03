// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model representing today's orders partitioned across Pending, Accepted,
//                  and Rejected tabs for the administrator order dashboard.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;

namespace FoodDeliveryApp.Models.ViewModels
{
    /// <summary>
    /// View model for the administrative order management dashboard.
    /// </summary>
    public class AdminOrdersViewModel
    {
        /// <summary>
        /// The currently active tab name: 'pending', 'accepted', or 'rejected'.
        /// </summary>
        public string ActiveTab { get; set; } = "pending";

        /// <summary>
        /// Orders in 'Pending' status placed today.
        /// </summary>
        public List<Order> PendingOrders { get; set; } = new List<Order>();

        /// <summary>
        /// Orders in 'Accepted' status placed today.
        /// </summary>
        public List<Order> AcceptedOrders { get; set; } = new List<Order>();

        /// <summary>
        /// Orders in 'Rejected' status placed today (manual or 5-min auto-rejection).
        /// </summary>
        public List<Order> RejectedOrders { get; set; } = new List<Order>();

        /// <summary>
        /// Number of seconds between client dashboard auto-refreshes.
        /// </summary>
        public int AutoRefreshSeconds { get; set; } = 60;

        /// <summary>
        /// Count of orders auto-rejected during the latest poll cycle.
        /// </summary>
        public int AutoRejectedCount { get; set; }
    }
}
