// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Repository interface contract for Order operations and XML item ingestion.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.Models.ViewModels;

namespace FoodDeliveryApp.Data.Repositories
{
    /// <summary>
    /// Contract for order creation, customer history, and admin order management.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Creates an order with line items ingested via XML in an atomic database transaction.
        /// </summary>
        /// <param name="userId">Customer User ID.</param>
        /// <param name="deliveryAddress">Destination delivery address snapshot.</param>
        /// <param name="contactPhone">Contact phone snapshot.</param>
        /// <param name="paymentMethod">Payment method.</param>
        /// <param name="totalAmount">Total payable amount.</param>
        /// <param name="items">Collection of line items to serialize to XML.</param>
        /// <returns>The generated OrderId.</returns>
        Task<int> CreateOrderAsync(
            int userId,
            string deliveryAddress,
            string contactPhone,
            string paymentMethod,
            decimal totalAmount,
            List<CartItemViewModel> items);

        /// <summary>
        /// Retrieves today's orders partitioned by status ('Pending', 'Accepted', or 'Rejected').
        /// </summary>
        /// <param name="status">Order status filter.</param>
        /// <returns>List of today's orders in that status.</returns>
        Task<List<Order>> GetTodayOrdersByStatusAsync(string status);

        /// <summary>
        /// Transitions a Pending order to Accepted or Rejected. Returns 1 if transitioned, 0 if already changed.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="newStatus">New status ('Accepted' or 'Rejected').</param>
        /// <returns>Rows affected count (1 or 0).</returns>
        Task<int> UpdateOrderStatusAsync(int orderId, string newStatus);

        /// <summary>
        /// SARGable auto-rejection of pending orders placed more than 5 minutes ago.
        /// </summary>
        /// <returns>Number of orders transitioned to Rejected.</returns>
        Task<int> AutoRejectPendingOrdersAsync();

        /// <summary>
        /// Retrieves past order history for a specific customer.
        /// </summary>
        /// <param name="userId">Customer User ID.</param>
        /// <returns>List of order history view models.</returns>
        Task<List<OrderHistoryItemViewModel>> GetCustomerHistoryAsync(int userId);
    }
}
