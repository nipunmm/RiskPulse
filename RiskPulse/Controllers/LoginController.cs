using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.DbModel.AccessControl;
using RiskPulse.Services.AccessControlService;
using RiskPulse.Services.LoginService;

namespace RiskPulse.Controllers
{
    public class LoginController : Controller
    {
        private readonly AdAuthenticationService _authService;
        private readonly DbAuthorizationService _authorizationService;

        public LoginController(AdAuthenticationService authService, DbAuthorizationService authorizationService)
        {
            _authService = authService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var defaultPage = User.FindFirst("DefaultPage")?.Value;
                var (controller, action) = PermissionPageMapper.GetRouteForPermission(defaultPage);
                return RedirectToAction(action, controller);
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
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

            var defaultPageDesc = user.Role?.DefaultPermission?.PermissionDesc ?? "Dashboard";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role?.RoleDesc ?? string.Empty),
                new Claim("DefaultPage", defaultPageDesc)
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

            var (targetController, targetAction) = PermissionPageMapper.GetRouteForPermission(defaultPageDesc);
            var redirectUrl = Url.Action(targetAction, targetController) ?? Url.Action("Index", "Dashboard");

            return Json(new { success = true, redirectUrl });
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
