using System.Security.Claims;

namespace RiskPulse.Models.Dto;

public class LoginResultDto
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public ClaimsPrincipal? Principal { get; set; }

    public string? RedirectController { get; set; }

    public string? RedirectAction { get; set; }
}