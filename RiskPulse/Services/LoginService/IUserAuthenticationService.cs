namespace RiskPulse.Services.LoginService;

public interface IUserAuthenticationService
{
    Task<bool> ValidateCredentialsAsync(string username, string password);
}
