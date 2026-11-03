// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Global HTTP Application entry point handling startup initialization and
//                  FormsAuthentication ticket role resolution in Application_PostAuthenticateRequest.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace FoodDeliveryApp
{
    /// <summary>
    /// Global application class orchestrating application lifecycle events.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Executes once when the application domain initializes.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Resolves authenticated user roles from the decrypted FormsAuthenticationTicket
        /// and constructs a GenericPrincipal for role-based authorization.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                try
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (ticket != null && !ticket.Expired)
                    {
                        string role = string.IsNullOrWhiteSpace(ticket.UserData) ? "Customer" : ticket.UserData.Trim();
                        var identity = new FormsIdentity(ticket);
                        var principal = new GenericPrincipal(identity, new[] { role });
                        HttpContext.Current.User = principal;
                        System.Threading.Thread.CurrentPrincipal = principal;
                    }
                }
                catch
                {
                    // Invalid, expired, or tampered cookie will be ignored
                }
            }
        }
    }
}
