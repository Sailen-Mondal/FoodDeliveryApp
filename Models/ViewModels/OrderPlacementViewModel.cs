// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model for customer order review and final placement at checkout.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Added multi-method payment fields (UPI, Card, NetBanking, COD) and IdempotencyToken.
// =========================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FoodDeliveryApp.Models.ViewModels
{
    /// <summary>
    /// View model for checkout and placing an order.
    /// </summary>
    public class OrderPlacementViewModel
    {
        /// <summary>
        /// Delivery destination address.
        /// </summary>
        [Required(ErrorMessage = "Delivery address is required.")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Primary contact phone number.
        /// </summary>
        [Required(ErrorMessage = "Contact phone number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit Indian mobile number.")]
        [Display(Name = "Contact Phone Number")]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Selected payment category: 'UPI', 'Card', 'NetBanking', or 'COD'.
        /// </summary>
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "UPI";

        /// <summary>
        /// Selected UPI App or sub-channel: 'GPay', 'PhonePe', 'Paytm', 'CRED', 'QR', or 'VPA'.
        /// </summary>
        public string UpiApp { get; set; } = "GPay";

        /// <summary>
        /// Virtual Payment Address for UPI manual entry (e.g. name@okhdfcbank).
        /// </summary>
        public string UpiVpa { get; set; }

        /// <summary>
        /// Cardholder name as printed on card.
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// Card number input (sanitized on server, only last 4 kept for receipt, never persisted raw).
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Card expiration date (MM/YY).
        /// </summary>
        public string CardExpiry { get; set; }

        /// <summary>
        /// Card CVV / security code (used strictly for transaction authorization, NEVER stored).
        /// </summary>
        public string CardCvv { get; set; }

        /// <summary>
        /// Card brand detected dynamically (e.g., Visa, Mastercard, RuPay).
        /// </summary>
        public string CardNetwork { get; set; } = "Visa";

        /// <summary>
        /// Selected Indian bank for NetBanking (e.g. HDFC, ICICI, SBI, Axis, Kotak).
        /// </summary>
        public string SelectedBank { get; set; } = "HDFC Bank";

        /// <summary>
        /// Client-side generated idempotency token ensuring duplicate clicks cannot produce duplicate orders.
        /// </summary>
        public string IdempotencyToken { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Items currently in the cart.
        /// </summary>
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        /// <summary>
        /// Subtotal of all items before taxes or delivery fee.
        /// </summary>
        public decimal SubTotal => Items?.Sum(i => i.TotalPrice) ?? 0m;

        /// <summary>
        /// Flat delivery charge (free if order > ₹500, otherwise ₹40).
        /// </summary>
        public decimal DeliveryFee => SubTotal > 500m || SubTotal == 0m ? 0m : 40m;

        /// <summary>
        /// Total order payable amount.
        /// </summary>
        public decimal TotalAmount => SubTotal + DeliveryFee;
    }
}
