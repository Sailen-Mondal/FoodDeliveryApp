// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     ADO.NET repository implementation for MenuItems and Categories via Stored Procedures.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Data.Repositories
{
    /// <summary>
    /// Implements menu catalog data operations strictly through SQL Server Stored Procedures.
    /// </summary>
    public class MenuItemRepository : IMenuItemRepository
    {
        /// <summary>
        /// Retrieves all active categories using dbo.usp_Category_GetAll.
        /// </summary>
        /// <returns>List of categories.</returns>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await DbHelper.ExecuteReaderAsync("dbo.usp_Category_GetAll", null, reader => new Category
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves available customer menu items using dbo.usp_MenuItem_GetAll.
        /// </summary>
        public async Task<List<MenuItem>> GetAllAsync(int? categoryId = null, bool? isVeg = null, string searchTerm = null)
        {
            var pCategory = new SqlParameter("@CategoryId", SqlDbType.Int) { Value = (object)categoryId ?? DBNull.Value };
            var pIsVeg = new SqlParameter("@IsVeg", SqlDbType.Bit) { Value = (object)isVeg ?? DBNull.Value };
            var pSearch = new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100) { Value = string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : (object)searchTerm.Trim() };

            return await DbHelper.ExecuteReaderAsync("dbo.usp_MenuItem_GetAll", new[] { pCategory, pIsVeg, pSearch }, MapMenuItem).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves all menu items for the admin dashboard using dbo.usp_MenuItem_GetAdminAll.
        /// </summary>
        public async Task<List<MenuItem>> GetAdminAllAsync()
        {
            return await DbHelper.ExecuteReaderAsync("dbo.usp_MenuItem_GetAdminAll", null, MapMenuItem).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a single menu item by ID using dbo.usp_MenuItem_GetById.
        /// </summary>
        public async Task<MenuItem> GetByIdAsync(int itemId)
        {
            var pItemId = new SqlParameter("@ItemId", SqlDbType.Int) { Value = itemId };
            var list = await DbHelper.ExecuteReaderAsync("dbo.usp_MenuItem_GetById", new[] { pItemId }, MapMenuItem).ConfigureAwait(false);
            return list.FirstOrDefault();
        }

        /// <summary>
        /// Toggles item availability using dbo.usp_MenuItem_ToggleAvailability.
        /// </summary>
        public async Task<bool> ToggleAvailabilityAsync(int itemId)
        {
            var pItemId = new SqlParameter("@ItemId", SqlDbType.Int) { Value = itemId };
            return await DbHelper.ExecuteScalarAsync<bool>("dbo.usp_MenuItem_ToggleAvailability", new[] { pItemId }).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates item price using dbo.usp_MenuItem_UpdatePrice.
        /// </summary>
        public async Task<int> UpdatePriceAsync(int itemId, decimal newPrice)
        {
            var pItemId = new SqlParameter("@ItemId", SqlDbType.Int) { Value = itemId };
            var pPrice = new SqlParameter("@NewPrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = newPrice };
            return await DbHelper.ExecuteScalarAsync<int>("dbo.usp_MenuItem_UpdatePrice", new[] { pItemId, pPrice }).ConfigureAwait(false);
        }

        private static MenuItem MapMenuItem(SqlDataReader reader)
        {
            return new MenuItem
            {
                ItemId = reader.GetInt32(reader.GetOrdinal("ItemId")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                IsVeg = reader.GetBoolean(reader.GetOrdinal("IsVeg")),
                IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                PrepTimeMinutes = reader.GetInt32(reader.GetOrdinal("PrepTimeMinutes")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
