// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Repository interface contract for User authentication and account persistence.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Threading.Tasks;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Data.Repositories
{
    /// <summary>
    /// Contract for User repository operations.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Registers a new user. Returns the newly created UserId, or -1 if the email is already in use.
        /// </summary>
        /// <param name="user">The user entity to persist.</param>
        /// <returns>New UserId or -1 if duplicate.</returns>
        Task<int> RegisterAsync(User user);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <returns>Matching User entity or null.</returns>
        Task<User> GetByEmailAsync(string email);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">Unique user ID.</param>
        /// <returns>Matching User entity or null.</returns>
        Task<User> GetByIdAsync(int userId);
    }
}
