namespace RiskPulse.Services.AuthService;

public interface IAuthService
{
    Task<bool> ValidateCredentialsAsync(string username, string password);
}
