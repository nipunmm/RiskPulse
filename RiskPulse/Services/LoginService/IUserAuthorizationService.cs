using RiskPulse.Models.DbModel.AccessControl;

namespace RiskPulse.Services.LoginService;

public interface IUserAuthorizationService
{
    Task<User?> GetUserDetailsAsync(string username);
}
