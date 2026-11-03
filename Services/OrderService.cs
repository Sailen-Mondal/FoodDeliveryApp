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
// 2026-11-03   Sailen Mondal   Implemented server-authoritative price check, transaction generation, and decoupled payment status.
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
        private readonly IMenuItemRepository _menuItemRepository;

        /// <summary>
        /// Initializes a new instance of OrderService with the specified repositories.
        /// </summary>
        /// <param name="orderRepository">Repository for order data access.</param>
        /// <param name="menuItemRepository">Repository for catalog price verification.</param>
        public OrderService(IOrderRepository orderRepository = null, IMenuItemRepository menuItemRepository = null)
        {
            _orderRepository = orderRepository ?? new OrderRepository();
            _menuItemRepository = menuItemRepository ?? new MenuItemRepository();
        }

        /// <summary>
        /// Validates checkout submission, enforces server-authoritative catalog pricing,
        /// generates safe payment transaction metadata, and persists the order via stored procedure.
        /// </summary>
        public async Task<int> PlaceOrderAsync(int userId, OrderPlacementViewModel model)
        {
            if (userId <= 0 || model?.Items == null || !model.Items.Any())
            {
                return 0;
            }

            // Server-Authoritative Price and Item Verification (Zero Client Price Tampering)
            var verifiedItems = new List<CartItemViewModel>();
            foreach (var item in model.Items)
            {
                var dbItem = await _menuItemRepository.GetByIdAsync(item.ItemId).ConfigureAwait(false);
                if (dbItem == null || !dbItem.IsAvailable)
                {
                    continue; // Discard unlisted or unavailable items
                }

                int safeQty = Math.Max(1, Math.Min(item.Quantity, 50));
                verifiedItems.Add(new CartItemViewModel
                {
                    ItemId = dbItem.ItemId,
                    Name = dbItem.Name,
                    Price = dbItem.Price, // Authoritative price from DB
                    Quantity = safeQty,
                    ImageUrl = dbItem.ImageUrl,
                    IsVeg = dbItem.IsVeg
                });
            }

            if (!verifiedItems.Any())
            {
                return 0;
            }

            decimal subTotal = verifiedItems.Sum(i => i.Quantity * i.Price);
            decimal deliveryFee = subTotal > 500m || subTotal == 0m ? 0m : 40m;
            decimal totalAmount = subTotal + deliveryFee;

            // Determine decoupled payment lifecycle status, transaction references, and masked details
            string rawMethod = (model.PaymentMethod ?? "COD").Trim();
            string paymentStatus;
            string paymentTransactionId;
            string maskedPaymentDetails;
            string paymentMethodDisplay;
            var rng = new Random();

            if (rawMethod.Equals("UPI", StringComparison.OrdinalIgnoreCase))
            {
                paymentStatus = "Paid";
                paymentTransactionId = $"UPI_TXN_{DateTime.Now:yyyyMMddHHmmss}_{rng.Next(100000, 999999)}";
                string upiApp = string.IsNullOrWhiteSpace(model.UpiApp) ? "UPI" : model.UpiApp.Trim();
                string vpa = !string.IsNullOrWhiteSpace(model.UpiVpa) ? model.UpiVpa.Trim() : $"{upiApp.ToLowerInvariant()}user@okbank";
                maskedPaymentDetails = $"UPI ({upiApp}) - {vpa}";
                paymentMethodDisplay = $"UPI - {upiApp}";
            }
            else if (rawMethod.Equals("Card", StringComparison.OrdinalIgnoreCase))
            {
                paymentStatus = "Paid";
                paymentTransactionId = $"CARD_TXN_{DateTime.Now:yyyyMMddHHmmss}_{rng.Next(100000, 999999)}";
                string network = string.IsNullOrWhiteSpace(model.CardNetwork) ? "Card" : model.CardNetwork.Trim();
                
                // Clean card number to extract last 4 digits only (Strict PCI-DSS: zero plaintext stored)
                string rawCard = (model.CardNumber ?? string.Empty).Replace(" ", "").Replace("-", "");
                string last4 = rawCard.Length >= 4 ? rawCard.Substring(rawCard.Length - 4) : "4242";
                maskedPaymentDetails = $"{network} ending in •••• {last4}";
                paymentMethodDisplay = $"{network} Card";
            }
            else if (rawMethod.Equals("NetBanking", StringComparison.OrdinalIgnoreCase))
            {
                paymentStatus = "Paid";
                paymentTransactionId = $"NB_TXN_{DateTime.Now:yyyyMMddHHmmss}_{rng.Next(100000, 999999)}";
                string bank = string.IsNullOrWhiteSpace(model.SelectedBank) ? "HDFC Bank" : model.SelectedBank.Trim();
                maskedPaymentDetails = $"{bank} Online Banking";
                paymentMethodDisplay = $"NetBanking ({bank})";
            }
            else // Cash on Delivery
            {
                paymentStatus = "Pending";
                paymentTransactionId = $"COD_REF_{DateTime.Now:yyyyMMddHHmmss}_{rng.Next(100000, 999999)}";
                maskedPaymentDetails = "Cash/UPI Payment at Doorstep";
                paymentMethodDisplay = "Cash on Delivery";
            }

            string idempotencyKey = !string.IsNullOrWhiteSpace(model.IdempotencyToken) 
                ? model.IdempotencyToken.Trim() 
                : Guid.NewGuid().ToString("N");

            return await _orderRepository.CreateOrderAsync(
                userId,
                model.DeliveryAddress?.Trim(),
                model.ContactPhone?.Trim(),
                paymentMethodDisplay,
                paymentStatus,
                paymentTransactionId,
                maskedPaymentDetails,
                idempotencyKey,
                totalAmount,
                verifiedItems
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

        /// <summary>
        /// Retrieves an order by ID strictly validating that the requesting user owns the order or is Admin (Anti-IDOR).
        /// </summary>
        public async Task<OrderHistoryItemViewModel> GetOrderByIdForUserAsync(int orderId, int userId, bool isAdmin)
        {
            if (orderId <= 0 || (userId <= 0 && !isAdmin))
            {
                return null;
            }

            return await _orderRepository.GetOrderByIdAsync(orderId, userId, isAdmin).ConfigureAwait(false);
        }
    }
}
