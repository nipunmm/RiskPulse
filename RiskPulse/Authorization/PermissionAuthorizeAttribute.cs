using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

namespace RiskPulse.Authorization
{
    // Checks that the current user holds a "Permission" claim matching the given
    // permission description (seeded from the riskpulse.Permissions table).
    public class PermissionAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public string Permission { get; }

        public PermissionAuthorizeAttribute(string permission)
        {
            Permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            if (!user.HasClaim("Permission", Permission))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Login", null);
            }
        }
    }
}
