// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Service interface contract for Order business operations and 5-min auto-rejection.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDeliveryApp.Models.ViewModels;

namespace FoodDeliveryApp.Services
{
    /// <summary>
    /// Service contract orchestrating order checkout, status lifecycle, and admin dashboards.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Validates cart contents and places an order in an atomic transaction.
        /// </summary>
        /// <param name="userId">Customer User ID.</param>
        /// <param name="model">Checkout placement details.</param>
        /// <returns>Created OrderId or 0 if failed.</returns>
        Task<int> PlaceOrderAsync(int userId, OrderPlacementViewModel model);

        /// <summary>
        /// Enforces the 5-minute auto-rejection rule and builds the complete AdminOrdersViewModel.
        /// </summary>
        /// <param name="activeTab">Target active tab name ('pending', 'accepted', or 'rejected').</param>
        /// <returns>Populated AdminOrdersViewModel.</returns>
        Task<AdminOrdersViewModel> GetAdminOrdersDashboardAsync(string activeTab);

        /// <summary>
        /// Updates an order status from Pending to Accepted or Rejected.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="newStatus">Target status ('Accepted' or 'Rejected').</param>
        /// <returns>True if transition succeeded; false if order was already changed or auto-rejected.</returns>
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);

        /// <summary>
        /// Triggers SARGable auto-rejection of pending orders placed more than 5 minutes ago.
        /// </summary>
        /// <returns>Count of auto-rejected orders.</returns>
        Task<int> CheckAndAutoRejectPendingOrdersAsync();

        /// <summary>
        /// Retrieves past order history for a customer.
        /// </summary>
        /// <param name="userId">Customer User ID.</param>
        /// <returns>List of past order summaries.</returns>
        Task<List<OrderHistoryItemViewModel>> GetCustomerOrderHistoryAsync(int userId);

        /// <summary>
        /// Retrieves order details by ID strictly validating that the requesting user owns the order or is Admin (Anti-IDOR).
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="userId">Requesting User ID.</param>
        /// <param name="isAdmin">Whether caller has Admin privileges.</param>
        /// <returns>Order details or null if unauthorized/not found.</returns>
        Task<OrderHistoryItemViewModel> GetOrderByIdForUserAsync(int orderId, int userId, bool isAdmin);
    }
}
