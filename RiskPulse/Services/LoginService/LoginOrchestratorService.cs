using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using RiskPulse.Models.AppModel;
using RiskPulse.Models.DbModel.AccessControl;
using RiskPulse.Services.AccessControlService;

namespace RiskPulse.Services.LoginService;

// Orchestrates the login flow: validates credentials (AD), loads the user's
// authorization details (DB), and builds the claims principal for the cookie.
public class LoginOrchestratorService
{
    private readonly AdAuthenticationService _adAuth;
    private readonly DbAuthorizationService _dbAuth;

    public LoginOrchestratorService(AdAuthenticationService adAuth, DbAuthorizationService dbAuth)
    {
        _adAuth = adAuth;
        _dbAuth = dbAuth;
    }

    public async Task<LoginResult> AuthenticateAsync(string username, string password)
    {
        if (!await _adAuth.ValidateCredentialsAsync(username, password))
        {
            return new LoginResult { Success = false, Message = "Authentication failed." };
        }

        var user = await _dbAuth.GetUserDetailsAsync(username);
        if (user == null)
        {
            return new LoginResult { Success = false, Message = "User not found in the system." };
        }

        if (!user.IsActive)
        {
            return new LoginResult { Success = false, Message = "User account is inactive." };
        }

        var defaultPageDesc = user.Role?.DefaultPermission?.PermissionDesc ?? PermissionCatalog.Dashboard;

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

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var (controller, action) = PermissionPageMapper.GetRouteForPermission(defaultPageDesc);

        return new LoginResult
        {
            Success = true,
            Principal = principal,
            RedirectController = controller,
            RedirectAction = action
        };
    }
}