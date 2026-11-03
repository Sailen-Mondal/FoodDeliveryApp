// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     ADO.NET repository implementation for User management via Stored Procedures.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Data.Repositories
{
    /// <summary>
    /// Implements user data access strictly using SQL Server Stored Procedures.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// Registers a new user via dbo.usp_User_Register.
        /// </summary>
        /// <param name="user">User entity containing registration data.</param>
        /// <returns>New UserId or -1 if email is duplicate.</returns>
        public async Task<int> RegisterAsync(User user)
        {
            var pFullName = new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = (object)user.FullName ?? DBNull.Value };
            var pEmail = new SqlParameter("@Email", SqlDbType.NVarChar, 150) { Value = (object)user.Email ?? DBNull.Value };
            var pPasswordHash = new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 256) { Value = (object)user.PasswordHash ?? DBNull.Value };
            var pPasswordSalt = new SqlParameter("@PasswordSalt", SqlDbType.NVarChar, 256) { Value = (object)user.PasswordSalt ?? DBNull.Value };
            var pPhoneNumber = new SqlParameter("@PhoneNumber", SqlDbType.NVarChar, 20) { Value = (object)user.PhoneNumber ?? DBNull.Value };
            var pAddress = new SqlParameter("@Address", SqlDbType.NVarChar, 300) { Value = (object)user.Address ?? DBNull.Value };
            var pRole = new SqlParameter("@Role", SqlDbType.NVarChar, 20) { Value = string.IsNullOrWhiteSpace(user.Role) ? "Customer" : (object)user.Role };
            var pNewUserId = new SqlParameter("@NewUserId", SqlDbType.Int) { Direction = ParameterDirection.Output };

            using (var conn = await DbHelper.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var cmd = new SqlCommand("dbo.usp_User_Register", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(pFullName);
                cmd.Parameters.Add(pEmail);
                cmd.Parameters.Add(pPasswordHash);
                cmd.Parameters.Add(pPasswordSalt);
                cmd.Parameters.Add(pPhoneNumber);
                cmd.Parameters.Add(pAddress);
                cmd.Parameters.Add(pRole);
                cmd.Parameters.Add(pNewUserId);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                if (pNewUserId.Value != null && pNewUserId.Value != DBNull.Value)
                {
                    return Convert.ToInt32(pNewUserId.Value);
                }
            }

            return -1;
        }

        /// <summary>
        /// Retrieves a user by email via dbo.usp_User_GetByEmail.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <returns>Matching User or null.</returns>
        public async Task<User> GetByEmailAsync(string email)
        {
            var pEmail = new SqlParameter("@Email", SqlDbType.NVarChar, 150) { Value = email ?? string.Empty };

            var list = await DbHelper.ExecuteReaderAsync("dbo.usp_User_GetByEmail", new[] { pEmail }, reader => new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                PasswordSalt = reader.GetString(reader.GetOrdinal("PasswordSalt")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            }).ConfigureAwait(false);

            return list.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a user by ID via dbo.usp_User_GetById.
        /// </summary>
        /// <param name="userId">User primary key identifier.</param>
        /// <returns>Matching User or null.</returns>
        public async Task<User> GetByIdAsync(int userId)
        {
            var pUserId = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };

            var list = await DbHelper.ExecuteReaderAsync("dbo.usp_User_GetById", new[] { pUserId }, reader => new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            }).ConfigureAwait(false);

            return list.FirstOrDefault();
        }
    }
}
