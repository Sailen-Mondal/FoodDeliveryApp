// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Service interface contract for Authentication, PBKDF2 password security,
//                  and FormsAuthentication ticket issuance.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Threading.Tasks;
using System.Web;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.Models.ViewModels;

namespace FoodDeliveryApp.Services
{
    /// <summary>
    /// Service contract for authentication workflows and security operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Validates login credentials and returns the matching user on success.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <param name="password">Plaintext password.</param>
        /// <returns>Authenticated User entity or null.</returns>
        Task<User> ValidateCredentialsAsync(string email, string password);

        /// <summary>
        /// Registers a new customer, hashing password using PBKDF2 with a fresh cryptographically random salt.
        /// </summary>
        /// <param name="model">Registration details.</param>
        /// <returns>New UserId or -1 if email is duplicate.</returns>
        Task<int> RegisterCustomerAsync(RegisterViewModel model);

        /// <summary>
        /// Creates an encrypted FormsAuthenticationTicket embedding user role in UserData and appends the auth cookie.
        /// </summary>
        /// <param name="user">User entity.</param>
        /// <param name="rememberMe">Whether to issue a persistent cookie.</param>
        /// <param name="response">HTTP response to append the cookie to.</param>
        void SignIn(User user, bool rememberMe, HttpResponseBase response);

        /// <summary>
        /// Safely terminates session and evicts the authentication cookie from client browser.
        /// </summary>
        /// <param name="response">HTTP response to append expired cookie to.</param>
        /// <param name="session">HTTP session to abandon.</param>
        void SignOut(HttpResponseBase response, HttpSessionStateBase session);
    }
}
