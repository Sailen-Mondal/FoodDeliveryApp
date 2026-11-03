// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Script and style bundle registrations for Food Delivery Application,
//                  incorporating Bootstrap 5 bundle (with embedded Popper.js) and Foodly theme.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Web.Optimization;

namespace FoodDeliveryApp
{
    /// <summary>
    /// Configures CSS and JavaScript script bundles for optimization and minification.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// Registers client script and stylesheet bundles.
        /// </summary>
        /// <param name="bundles">The bundle collection to configure.</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Use bootstrap.bundle.js which includes Popper.js to enable Bootstrap 5 dropdowns and popovers
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.bundle.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/foodly.css",
                        "~/Content/Site.css"));
        }
    }
}
