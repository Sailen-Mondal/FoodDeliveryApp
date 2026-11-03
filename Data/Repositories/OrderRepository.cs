// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     ADO.NET repository implementation for Orders via Stored Procedures and XML payload.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Upgraded CreateOrderAsync with payment metadata and mapped payment fields.
// =========================================================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.Models.ViewModels;

namespace FoodDeliveryApp.Data.Repositories
{
    /// <summary>
    /// Implements order management strictly through SQL Server Stored Procedures.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        /// <summary>
        /// Ingests cart items via XML and executes dbo.usp_Order_Create inside a database transaction.
        /// </summary>
        public async Task<int> CreateOrderAsync(
            int userId,
            string deliveryAddress,
            string contactPhone,
            string paymentMethod,
            string paymentStatus,
            string paymentTransactionId,
            string maskedPaymentDetails,
            string idempotencyKey,
            decimal totalAmount,
            List<CartItemViewModel> items)
        {
            var xmlDoc = new XElement("items",
                items.Select(i => new XElement("item",
                    new XAttribute("ItemId", i.ItemId),
                    new XAttribute("ItemName", i.Name ?? string.Empty),
                    new XAttribute("UnitPrice", i.Price),
                    new XAttribute("Quantity", i.Quantity)
                ))
            );

            var pUserId = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
            var pAddress = new SqlParameter("@DeliveryAddress", SqlDbType.NVarChar, 300) { Value = deliveryAddress ?? string.Empty };
            var pPhone = new SqlParameter("@ContactPhone", SqlDbType.NVarChar, 20) { Value = contactPhone ?? string.Empty };
            var pPaymentMethod = new SqlParameter("@PaymentMethod", SqlDbType.NVarChar, 50) { Value = paymentMethod ?? "Cash on Delivery" };
            var pPaymentStatus = new SqlParameter("@PaymentStatus", SqlDbType.NVarChar, 20) { Value = paymentStatus ?? "Pending" };
            var pTxnId = new SqlParameter("@PaymentTransactionId", SqlDbType.NVarChar, 100) { Value = (object)paymentTransactionId ?? DBNull.Value };
            var pMasked = new SqlParameter("@MaskedPaymentDetails", SqlDbType.NVarChar, 100) { Value = (object)maskedPaymentDetails ?? DBNull.Value };
            var pIdempotency = new SqlParameter("@IdempotencyKey", SqlDbType.NVarChar, 100) { Value = (object)idempotencyKey ?? DBNull.Value };
            var pTotal = new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = totalAmount };
            var pXml = new SqlParameter("@OrderItemsXml", SqlDbType.Xml) { Value = xmlDoc.ToString() };

            using (var conn = await DbHelper.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var cmd = new SqlCommand("dbo.usp_Order_Create", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(pUserId);
                cmd.Parameters.Add(pAddress);
                cmd.Parameters.Add(pPhone);
                cmd.Parameters.Add(pPaymentMethod);
                cmd.Parameters.Add(pPaymentStatus);
                cmd.Parameters.Add(pTxnId);
                cmd.Parameters.Add(pMasked);
                cmd.Parameters.Add(pIdempotency);
                cmd.Parameters.Add(pTotal);
                cmd.Parameters.Add(pXml);

                var scalarResult = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (scalarResult != null && scalarResult != DBNull.Value)
                {
                    return Convert.ToInt32(scalarResult);
                }
            }

            return 0;
        }

        /// <summary>
        /// Retrieves today's orders partitioned by status using dbo.usp_Order_GetTodayOrdersByStatus.
        /// </summary>
        public async Task<List<Order>> GetTodayOrdersByStatusAsync(string status)
        {
            var pStatus = new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = status ?? "Pending" };

            return await DbHelper.ExecuteReaderAsync("dbo.usp_Order_GetTodayOrdersByStatus", new[] { pStatus }, reader => new Order
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
                ContactPhone = reader.GetString(reader.GetOrdinal("ContactPhone")),
                DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                PaymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? "Pending" : reader.GetString(reader.GetOrdinal("PaymentStatus")),
                PaymentTransactionId = reader.IsDBNull(reader.GetOrdinal("PaymentTransactionId")) ? null : reader.GetString(reader.GetOrdinal("PaymentTransactionId")),
                MaskedPaymentDetails = reader.IsDBNull(reader.GetOrdinal("MaskedPaymentDetails")) ? null : reader.GetString(reader.GetOrdinal("MaskedPaymentDetails")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                PlacedAt = reader.GetDateTime(reader.GetOrdinal("PlacedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                ItemsSummary = reader.IsDBNull(reader.GetOrdinal("ItemsSummary")) ? string.Empty : reader.GetString(reader.GetOrdinal("ItemsSummary"))
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an order status from Pending to Accepted or Rejected using dbo.usp_Order_UpdateStatus.
        /// </summary>
        public async Task<int> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var pOrderId = new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId };
            var pNewStatus = new SqlParameter("@NewStatus", SqlDbType.NVarChar, 20) { Value = newStatus };

            return await DbHelper.ExecuteScalarAsync<int>("dbo.usp_Order_UpdateStatus", new[] { pOrderId, pNewStatus }).ConfigureAwait(false);
        }

        /// <summary>
        /// Auto-rejects pending orders placed more than 5 minutes ago using dbo.usp_Order_AutoRejectPending.
        /// </summary>
        public async Task<int> AutoRejectPendingOrdersAsync()
        {
            return await DbHelper.ExecuteScalarAsync<int>("dbo.usp_Order_AutoRejectPending", null).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves customer order history using dbo.usp_Order_GetCustomerHistory.
        /// </summary>
        public async Task<List<OrderHistoryItemViewModel>> GetCustomerHistoryAsync(int userId)
        {
            var pUserId = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };

            return await DbHelper.ExecuteReaderAsync("dbo.usp_Order_GetCustomerHistory", new[] { pUserId }, reader => new OrderHistoryItemViewModel
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                ContactPhone = reader.GetString(reader.GetOrdinal("ContactPhone")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                PaymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? "Pending" : reader.GetString(reader.GetOrdinal("PaymentStatus")),
                PaymentTransactionId = reader.IsDBNull(reader.GetOrdinal("PaymentTransactionId")) ? null : reader.GetString(reader.GetOrdinal("PaymentTransactionId")),
                MaskedPaymentDetails = reader.IsDBNull(reader.GetOrdinal("MaskedPaymentDetails")) ? null : reader.GetString(reader.GetOrdinal("MaskedPaymentDetails")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                PlacedAt = reader.GetDateTime(reader.GetOrdinal("PlacedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                ItemsSummary = reader.IsDBNull(reader.GetOrdinal("ItemsSummary")) ? string.Empty : reader.GetString(reader.GetOrdinal("ItemsSummary"))
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves order details by OrderId strictly enforcing authorization (Anti-IDOR) via dbo.usp_Order_GetById.
        /// </summary>
        public async Task<OrderHistoryItemViewModel> GetOrderByIdAsync(int orderId, int userId, bool isAdmin)
        {
            var pOrderId = new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId };
            var pUserId = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
            var pIsAdmin = new SqlParameter("@IsAdmin", SqlDbType.Bit) { Value = isAdmin };

            var list = await DbHelper.ExecuteReaderAsync("dbo.usp_Order_GetById", new[] { pOrderId, pUserId, pIsAdmin }, reader => new OrderHistoryItemViewModel
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                ContactPhone = reader.GetString(reader.GetOrdinal("ContactPhone")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                PaymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? "Pending" : reader.GetString(reader.GetOrdinal("PaymentStatus")),
                PaymentTransactionId = reader.IsDBNull(reader.GetOrdinal("PaymentTransactionId")) ? null : reader.GetString(reader.GetOrdinal("PaymentTransactionId")),
                MaskedPaymentDetails = reader.IsDBNull(reader.GetOrdinal("MaskedPaymentDetails")) ? null : reader.GetString(reader.GetOrdinal("MaskedPaymentDetails")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                PlacedAt = reader.GetDateTime(reader.GetOrdinal("PlacedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                ItemsSummary = reader.IsDBNull(reader.GetOrdinal("ItemsSummary")) ? string.Empty : reader.GetString(reader.GetOrdinal("ItemsSummary"))
            }).ConfigureAwait(false);

            return list.FirstOrDefault();
        }
    }
}
