// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model for customer order review and final placement at checkout.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

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
        /// Payment method selected: 'Cash on Delivery' or 'UPI on Delivery'.
        /// </summary>
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

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
