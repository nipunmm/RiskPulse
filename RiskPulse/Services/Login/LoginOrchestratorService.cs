using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using RiskPulse.Models.Dto;

namespace RiskPulse.Services.Login;

// --- Login flow: validate AD, load user, build cookie claims ---
public class LoginOrchestratorService
{
    private readonly AdAuthenticationService _adAuth;
    private readonly DbAuthorizationService _dbAuth;

    public LoginOrchestratorService(AdAuthenticationService adAuth, DbAuthorizationService dbAuth)
    {
        _adAuth = adAuth;
        _dbAuth = dbAuth;
    }

    public async Task<LoginResultDto> AuthenticateAsync(string username, string password)
    {
        if (!await _adAuth.ValidateCredentialsAsync(username, password))
        {
            return new LoginResultDto { Success = false, Message = "Authentication failed." };
        }

        var user = await _dbAuth.GetUserDetailsAsync(username);
        if (user == null)
        {
            return new LoginResultDto { Success = false, Message = "User not found in the system." };
        }

        if (!user.IsActive)
        {
            return new LoginResultDto { Success = false, Message = "User account is inactive." };
        }

        var defaultPageDesc = user.DefaultPermissionDesc ?? PermissionCatalog.Dashboard;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.RoleDesc ?? string.Empty),
            new Claim("DefaultPage", defaultPageDesc)
        };

        if (!string.IsNullOrEmpty(user.UnitDesc))
        {
            claims.Add(new Claim("Unit", user.UnitDesc));
        }

        foreach (var perm in user.PermissionDescs)
        {
            claims.Add(new Claim("Permission", perm));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var (controller, action) = PermissionPageMapper.GetRouteForPermission(defaultPageDesc);

        return new LoginResultDto
        {
            Success = true,
            Principal = principal,
            RedirectController = controller,
            RedirectAction = action
        };
    }
}
