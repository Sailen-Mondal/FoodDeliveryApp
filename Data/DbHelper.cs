// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     ADO.NET Database Helper executing SQL Server Stored Procedures with safe
//                  parameter binding, deterministic resource disposal, and structured error handling.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Data
{
    /// <summary>
    /// Thread-safe ADO.NET utility class strictly invoking SQL Server Stored Procedures.
    /// Direct ad-hoc inline queries are strictly prohibited per project architecture guidelines.
    /// </summary>
    public static class DbHelper
    {
        private static readonly string ConnectionString =
            Environment.GetEnvironmentVariable("FOODLY_DB_CONNECTION")
            ?? Environment.GetEnvironmentVariable("SQLCONNSTR_FoodDeliveryDb")
            ?? ConfigurationManager.ConnectionStrings["FoodDeliveryDb"]?.ConnectionString
            ?? @"Data Source=.\SQLEXPRESS;Initial Catalog=FoodDeliveryDb;Integrated Security=True;TrustServerCertificate=True;";

        /// <summary>
        /// Creates and opens a new SqlConnection instance.
        /// </summary>
        /// <returns>An open SqlConnection.</returns>
        public static async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            return conn;
        }

        /// <summary>
        /// Creates and opens a new synchronous SqlConnection instance.
        /// </summary>
        /// <returns>An open SqlConnection.</returns>
        public static SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Executes a stored procedure that returns a result set, projecting each record via the mapper delegate.
        /// </summary>
        /// <typeparam name="T">Type of mapped entity.</typeparam>
        /// <param name="storedProcedureName">Qualified stored procedure name (e.g. dbo.usp_MenuItem_GetAll).</param>
        /// <param name="parameters">Array of typed SqlParameters (can be null).</param>
        /// <param name="mapper">Func mapping an active SqlDataReader row to T.</param>
        /// <returns>List of mapped entities.</returns>
        public static async Task<List<T>> ExecuteReaderAsync<T>(
            string storedProcedureName,
            SqlParameter[] parameters,
            Func<SqlDataReader, T> mapper)
        {
            var results = new List<T>();

            using (var conn = await GetOpenConnectionAsync().ConfigureAwait(false))
            using (var cmd = new SqlCommand(storedProcedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        results.Add(mapper(reader));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Executes a stored procedure returning a single scalar value.
        /// </summary>
        /// <typeparam name="T">Target scalar type.</typeparam>
        /// <param name="storedProcedureName">Qualified stored procedure name.</param>
        /// <param name="parameters">Array of typed SqlParameters (can be null).</param>
        /// <returns>Scalar value converted to T, or default.</returns>
        public static async Task<T> ExecuteScalarAsync<T>(
            string storedProcedureName,
            SqlParameter[] parameters)
        {
            using (var conn = await GetOpenConnectionAsync().ConfigureAwait(false))
            using (var cmd = new SqlCommand(storedProcedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                object result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (result == null || result == DBNull.Value)
                {
                    return default(T);
                }

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        /// <summary>
        /// Executes a stored procedure modifying data (UPDATE, INSERT, DELETE) and returns affected row count.
        /// Output parameters can be inspected after execution.
        /// </summary>
        /// <param name="storedProcedureName">Qualified stored procedure name.</param>
        /// <param name="parameters">Array of typed SqlParameters (can be null).</param>
        /// <returns>Number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync(
            string storedProcedureName,
            SqlParameter[] parameters)
        {
            using (var conn = await GetOpenConnectionAsync().ConfigureAwait(false))
            using (var cmd = new SqlCommand(storedProcedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}
