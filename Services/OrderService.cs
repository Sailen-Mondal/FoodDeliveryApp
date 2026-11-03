// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Service implementation handling Order workflows, auto-rejection rule enforcement,
//                  and order dashboard status partitioning.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDeliveryApp.Data.Repositories;
using FoodDeliveryApp.Models.ViewModels;

namespace FoodDeliveryApp.Services
{
    /// <summary>
    /// Implements order business logic including automatic 5-minute timeout rejection.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Initializes a new instance of OrderService with the specified repository.
        /// </summary>
        /// <param name="orderRepository">Repository for order data access.</param>
        public OrderService(IOrderRepository orderRepository = null)
        {
            _orderRepository = orderRepository ?? new OrderRepository();
        }

        /// <summary>
        /// Validates checkout submission and persists the order via stored procedure.
        /// </summary>
        public async Task<int> PlaceOrderAsync(int userId, OrderPlacementViewModel model)
        {
            if (userId <= 0 || model?.Items == null || !model.Items.Any())
            {
                return 0;
            }

            decimal subTotal = model.Items.Sum(i => i.TotalPrice);
            decimal deliveryFee = subTotal > 500m || subTotal == 0m ? 0m : 40m;
            decimal totalAmount = subTotal + deliveryFee;

            return await _orderRepository.CreateOrderAsync(
                userId,
                model.DeliveryAddress?.Trim(),
                model.ContactPhone?.Trim(),
                model.PaymentMethod ?? "Cash on Delivery",
                totalAmount,
                model.Items
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Auto-rejects pending orders older than 5 minutes and loads today's partitioned orders.
        /// </summary>
        public async Task<AdminOrdersViewModel> GetAdminOrdersDashboardAsync(string activeTab)
        {
            // Enforce automatic 5-minute pending order rejection before querying dashboard
            int autoRejectedCount = await _orderRepository.AutoRejectPendingOrdersAsync().ConfigureAwait(false);

            string normalizedTab = (activeTab ?? "pending").Trim().ToLowerInvariant();
            if (normalizedTab != "pending" && normalizedTab != "accepted" && normalizedTab != "rejected")
            {
                normalizedTab = "pending";
            }

            // Retrieve today's orders partitioned across all 3 tabs concurrently
            var pendingTask = _orderRepository.GetTodayOrdersByStatusAsync("Pending");
            var acceptedTask = _orderRepository.GetTodayOrdersByStatusAsync("Accepted");
            var rejectedTask = _orderRepository.GetTodayOrdersByStatusAsync("Rejected");

            await Task.WhenAll(pendingTask, acceptedTask, rejectedTask).ConfigureAwait(false);

            return new AdminOrdersViewModel
            {
                ActiveTab = normalizedTab,
                PendingOrders = pendingTask.Result,
                AcceptedOrders = acceptedTask.Result,
                RejectedOrders = rejectedTask.Result,
                AutoRefreshSeconds = 60,
                AutoRejectedCount = autoRejectedCount
            };
        }

        /// <summary>
        /// Transitions a Pending order to Accepted or Rejected.
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            if (orderId <= 0 || (newStatus != "Accepted" && newStatus != "Rejected"))
            {
                return false;
            }

            int rows = await _orderRepository.UpdateOrderStatusAsync(orderId, newStatus).ConfigureAwait(false);
            return rows > 0;
        }

        /// <summary>
        /// Executes SARGable auto-rejection on demand.
        /// </summary>
        public async Task<int> CheckAndAutoRejectPendingOrdersAsync()
        {
            return await _orderRepository.AutoRejectPendingOrdersAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves past order history for a customer.
        /// </summary>
        public async Task<List<OrderHistoryItemViewModel>> GetCustomerOrderHistoryAsync(int userId)
        {
            if (userId <= 0)
            {
                return new List<OrderHistoryItemViewModel>();
            }

            return await _orderRepository.GetCustomerHistoryAsync(userId).ConfigureAwait(false);
        }
    }
}
