namespace RiskPulse.Services.LoginService;

// Stub for Active Directory authentication.
// TODO: replace with a real directory/identity provider lookup.
public class AdAuthenticationService
{
    public Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        return Task.FromResult(true);
    }
}
