using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.DbModel.AccessControl;
using RiskPulse.Services.LoginService;
using RiskPulse.Validation;

namespace RiskPulse.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserAuthenticationService _authService;
        private readonly IUserAuthorizationService _authorizationService;

        public LoginController(IUserAuthenticationService authService, IUserAuthorizationService authorizationService)
        {
            _authService = authService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (!UsernameValidator.IsValid(username))
            {
                return Json(new { success = false, message = UsernameValidator.ErrorMessage });
            }

            if (!await _authService.ValidateCredentialsAsync(username, password))
            {
                return Json(new { success = false, message = "Authentication failed." });
            }

            var user = await _authorizationService.GetUserDetailsAsync(username);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found in the system." });
            }

            if (!user.IsActive)
            {
                return Json(new { success = false, message = "User account is inactive." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role?.RoleDesc ?? string.Empty)
            };

            if (user.Unit != null)
            {
                claims.Add(new Claim("Unit", user.Unit.UnitDesc));
            }

            foreach (var rolePermission in user.Role?.RolePermissions ?? Enumerable.Empty<RolePermission>())
            {
                if (rolePermission.Permission != null)
                {
                    claims.Add(new Claim("Permission", rolePermission.Permission.PermissionDesc));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Dashboard") });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            ViewData["RequestId"] = HttpContext.TraceIdentifier;
            return View();
        }
    }
}
