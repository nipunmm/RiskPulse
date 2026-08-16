namespace RiskPulse.Services.Login;

// --- AD auth stub (always validates; TODO: real provider) ---
public class AdAuthenticationService
{
    public Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        return Task.FromResult(true);
    }
}
