namespace RiskPulse.Services.AuthService;

public class DevAuthService : IAuthService
{
    public Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        var isValid = username == "admin" && password == "1234";
        return Task.FromResult(isValid);
    }
}
