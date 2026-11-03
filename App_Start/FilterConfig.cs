// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Global MVC action filter registration.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Web.Mvc;

namespace FoodDeliveryApp
{
    /// <summary>
    /// Configures global action filters applied across all controllers.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers global filters.
        /// </summary>
        /// <param name="filters">Global filter collection.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
