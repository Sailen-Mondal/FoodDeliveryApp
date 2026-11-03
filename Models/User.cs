// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Domain entity representing an application user (Customer or Admin).
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;

namespace FoodDeliveryApp.Models
{
    /// <summary>
    /// Represents a registered user in the Food Delivery system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique primary key identifier for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// User's email address, used as login username.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Salted PBKDF2 hash of the user's password.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Cryptographic salt used during hashing.
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// 10-digit Indian contact phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Default physical delivery address in India.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Role assigned to the user ('Customer' or 'Admin').
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Date and time when the user registered.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
