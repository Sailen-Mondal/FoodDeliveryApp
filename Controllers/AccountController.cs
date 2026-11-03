// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     Controller managing Customer and Administrator registration, authentication,
//                  and secure sign-out.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// 2026-11-03   Sailen Mondal   Added parameterless constructor for DefaultControllerActivator.
// =========================================================================================

using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using FoodDeliveryApp.Infrastructure.Filters;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.Models.ViewModels;
using FoodDeliveryApp.Services;

namespace FoodDeliveryApp.Controllers
{
    /// <summary>
    /// Handles user registration, credentials verification, and session sign-out.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of AccountController with default authentication service.
        /// </summary>
        public AccountController() : this(new AuthService())
        {
        }

        /// <summary>
        /// Initializes a new instance of AccountController with injected authentication service.
        /// </summary>
        /// <param name="authService">Authentication service instance.</param>
        public AccountController(IAuthService authService)
        {
            _authService = authService ?? new AuthService();
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RateLimit(Seconds = 60, MaxRequests = 5, Message = "Too many registration attempts. Please wait 60 seconds before trying again.")]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int newUserId = await _authService.RegisterCustomerAsync(model);
                if (newUserId == -1)
                {
                    ModelState.AddModelError("Email", "An account with this email address already exists.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Your account has been created successfully! Please sign in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Registration failed: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RateLimit(Seconds = 60, MaxRequests = 6, Message = "Too many login attempts. For security reasons, please wait 60 seconds before trying again.")]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await _authService.ValidateCredentialsAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email address or password.");
                return View(model);
            }

            _authService.SignIn(user, model.RememberMe, Response);

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Orders", "Admin");
            }

            return RedirectToLocal(model.ReturnUrl);
        }

        // GET: /Account/AdminLogin
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AdminLogin(string returnUrl)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("Orders", "Admin");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/AdminLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RateLimit(Seconds = 60, MaxRequests = 5, Message = "Too many administrator login attempts. For security reasons, please wait 60 seconds before trying again.")]
        public async Task<ActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await _authService.ValidateCredentialsAsync(model.Email, model.Password);
            if (user == null || !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Invalid administrator credentials or unauthorized role.");
                return View(model);
            }

            _authService.SignIn(user, model.RememberMe, Response);
            return RedirectToAction("Orders", "Admin");
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _authService.SignOut(Response, Session);
            TempData["SuccessMessage"] = "You have been signed out safely.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/LogOff (Safe fallback to prevent 404/405 errors)
        [HttpGet]
        [ActionName("LogOff")]
        public ActionResult LogOffGet()
        {
            _authService.SignOut(Response, Session);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout (Friendly route alias)
        [HttpGet]
        public ActionResult Logout()
        {
            _authService.SignOut(Response, Session);
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
