// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model capturing login credentials for customer and admin authentication.
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
    /// View model for user login form.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// User's secret password.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Flag to issue persistent authentication cookie.
        /// </summary>
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Return URL to redirect to upon successful sign in.
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}
