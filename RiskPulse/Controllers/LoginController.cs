using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginOrchestratorService _loginService;

        public LoginController(LoginOrchestratorService loginService)
        {
            _loginService = loginService;
        }

        // --- Login (page + submit) ---
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
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null || !ModelState.IsValid)
            {
                var message = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Please correct the form errors and try again.";

                return Json(ApiResponse.Fail<object>(message));
            }

            var result = await _loginService.AuthenticateAsync(request.Username, request.Password);
            if (!result.Success)
            {
                return Json(ApiResponse.Fail<object>(result.Message ?? "Authentication failed."));
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal!);

            var redirectUrl = Url.Action(result.RedirectAction, result.RedirectController) ?? Url.Action("Index", "Dashboard");
            return Json(ApiResponse.Ok(new { redirectUrl }));
        }

        // --- Logout + access denied ---
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