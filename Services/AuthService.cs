// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Service implementation handling PBKDF2 cryptographic hashing, credentials
//                  verification, FormsAuthentication ticket issuance, and safe sign-out.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using FoodDeliveryApp.Data.Repositories;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.Models.ViewModels;

namespace FoodDeliveryApp.Services
{
    /// <summary>
    /// Provides secure authentication workflows and cryptographic hashing.
    /// </summary>
    public class AuthService : IAuthService
    {
        private const int Iterations = 10000;
        private const int HashByteCount = 32;
        private const int SaltByteCount = 16;

        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of AuthService with required repositories.
        /// </summary>
        /// <param name="userRepository">Repository for user entity operations.</param>
        public AuthService(IUserRepository userRepository = null)
        {
            _userRepository = userRepository ?? new UserRepository();
        }

        /// <summary>
        /// Validates login credentials against stored PBKDF2 hash and salt.
        /// </summary>
        public async Task<User> ValidateCredentialsAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = await _userRepository.GetByEmailAsync(email.Trim()).ConfigureAwait(false);
            if (user == null)
            {
                return null;
            }

            bool isValid = VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
            return isValid ? user : null;
        }

        /// <summary>
        /// Registers a new customer with a generated salt and PBKDF2 password hash.
        /// </summary>
        public async Task<int> RegisterCustomerAsync(RegisterViewModel model)
        {
            string salt = GenerateSalt();
            string hash = HashPassword(model.Password, salt);

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PasswordHash = hash,
                PasswordSalt = salt,
                PhoneNumber = model.PhoneNumber?.Trim(),
                Address = model.Address?.Trim(),
                Role = "Customer"
            };

            return await _userRepository.RegisterAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Issues an encrypted FormsAuthentication ticket embedding the user's role.
        /// </summary>
        public void SignIn(User user, bool rememberMe, HttpResponseBase response)
        {
            DateTime now = DateTime.Now;
            DateTime expiration = rememberMe ? now.AddDays(14) : now.AddMinutes(120);

            var ticket = new FormsAuthenticationTicket(
                1,
                user.Email,
                now,
                expiration,
                rememberMe,
                user.Role ?? "Customer",
                FormsAuthentication.FormsCookiePath
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (rememberMe)
            {
                authCookie.Expires = ticket.Expiration;
            }

            response.Cookies.Add(authCookie);
        }

        /// <summary>
        /// Terminates authentication and session state and evicts the auth cookie.
        /// </summary>
        public void SignOut(HttpResponseBase response, HttpSessionStateBase session)
        {
            FormsAuthentication.SignOut();

            if (session != null)
            {
                session.Clear();
                session.Abandon();
            }

            var expiredCookie = new HttpCookie(FormsAuthentication.FormsCookieName, string.Empty)
            {
                Expires = DateTime.Now.AddYears(-1),
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };
            response.Cookies.Add(expiredCookie);
        }

        /// <summary>
        /// Generates a cryptographically random salt encoded in Base64.
        /// </summary>
        private static string GenerateSalt()
        {
            byte[] salt = new byte[SaltByteCount];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// Computes a PBKDF2 SHA-1 derived key matching the database seed generation.
        /// </summary>
        private static string HashPassword(string password, string base64Salt)
        {
            byte[] saltBytes = Convert.FromBase64String(base64Salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashByteCount);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verifies a password against the stored hash and salt using constant-time comparison.
        /// </summary>
        private static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            try
            {
                string computedHash = HashPassword(enteredPassword, storedSalt);
                byte[] a = Convert.FromBase64String(computedHash);
                byte[] b = Convert.FromBase64String(storedHash);

                return ConstantTimeEquals(a, b);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two byte arrays in constant time to prevent timing attacks.
        /// </summary>
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
