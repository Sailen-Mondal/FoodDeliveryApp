// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model for new customer account registration.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApp.Models.ViewModels
{
    /// <summary>
    /// View model capturing user input during customer registration.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Customer's full name.
        /// </summary>
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        /// <summary>
        /// Valid email address.
        /// </summary>
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// Account password.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation matching the entered password.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// 10-digit Indian phone number.
        /// </summary>
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit Indian mobile number (e.g. 9876543210).")]
        [Display(Name = "Mobile Number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Street address, city, state, and PIN code.
        /// </summary>
        [Required(ErrorMessage = "Delivery address is required.")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [Display(Name = "Delivery Address")]
        public string Address { get; set; }
    }
}
