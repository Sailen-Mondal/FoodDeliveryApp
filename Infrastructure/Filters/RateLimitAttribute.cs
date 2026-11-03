// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Defensive MVC action filter enforcing IP-based sliding window rate limiting
//                  to prevent credential stuffing, brute force, and automated spam submission.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace FoodDeliveryApp.Infrastructure.Filters
{
    /// <summary>
    /// Action filter that restricts the number of requests a client IP can make to an action
    /// within a specified time window, returning HTTP 429 Too Many Requests upon breach.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Sliding window duration in seconds. Default is 60 seconds.
        /// </summary>
        public int Seconds { get; set; } = 60;

        /// <summary>
        /// Maximum allowed requests within the time window. Default is 5.
        /// </summary>
        public int MaxRequests { get; set; } = 5;

        /// <summary>
        /// Custom user-facing message displayed upon rate limit breach.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Intercepts incoming requests to evaluate request volume against the defined rate threshold.
        /// </summary>
        /// <param name="filterContext">MVC action executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            string clientIp = GetClientIpAddress(request);
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = filterContext.ActionDescriptor.ActionName;

            string cacheKey = $"Foodly_RateLimit_{controllerName}_{actionName}_{clientIp}";

            var cache = HttpRuntime.Cache;
            object cachedValue = cache.Get(cacheKey);

            int currentCount = 0;
            if (cachedValue != null && int.TryParse(cachedValue.ToString(), out int val))
            {
                currentCount = val;
            }

            if (currentCount >= MaxRequests)
            {
                string breachMessage = !string.IsNullOrWhiteSpace(Message)
                    ? Message
                    : $"Too many requests. For your security, please wait {Seconds} seconds before trying again.";

                filterContext.HttpContext.Response.StatusCode = 429;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

                // Support asynchronous AJAX requests cleanly
                if (request.IsAjaxRequest() || (request.ContentType != null && request.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { success = false, message = breachMessage, isRateLimited = true },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }

                // For standard form submissions, add model error to view
                filterContext.Controller.ViewData.ModelState.AddModelError(string.Empty, breachMessage);
                
                // Set result to view so execution stops without executing the action logic
                filterContext.Result = new ViewResult
                {
                    ViewName = actionName,
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };
                return;
            }

            // Increment request count within the caching window
            if (currentCount == 0)
            {
                cache.Insert(
                    cacheKey,
                    1,
                    null,
                    DateTime.UtcNow.AddSeconds(Seconds),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Default,
                    null
                );
            }
            else
            {
                cache.Insert(
                    cacheKey,
                    currentCount + 1,
                    null,
                    DateTime.UtcNow.AddSeconds(Seconds),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Default,
                    null
                );
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Resolves the actual client IP address taking reverse proxies and cloud load balancers into account.
        /// </summary>
        private static string GetClientIpAddress(HttpRequestBase request)
        {
            try
            {
                // Check Cloudflare or reverse proxy header
                string cfIp = request.ServerVariables["HTTP_CF_CONNECTING_IP"];
                if (!string.IsNullOrWhiteSpace(cfIp))
                {
                    return cfIp.Trim();
                }

                // Check standard X-Forwarded-For header
                string forwardedFor = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                    // If multiple proxies exist, the first IP is the original client
                    string[] ips = forwardedFor.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (ips.Length > 0)
                    {
                        return ips[0].Trim();
                    }
                }

                return request.UserHostAddress ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}
