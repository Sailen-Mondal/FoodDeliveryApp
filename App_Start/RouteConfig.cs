// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     MVC route definitions including default routes and friendly route aliases.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Web.Mvc;
using System.Web.Routing;

namespace FoodDeliveryApp
{
    /// <summary>
    /// Configures application URL routing.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers URL routes with the global route collection.
        /// </summary>
        /// <param name="routes">Route collection instance.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Route alias for friendly Logout navigation
            routes.MapRoute(
                name: "LogoutAlias",
                url: "Account/Logout",
                defaults: new { controller = "Account", action = "LogOff" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
