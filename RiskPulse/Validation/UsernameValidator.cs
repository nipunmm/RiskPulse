using System.Text.RegularExpressions;

namespace RiskPulse.Validation
{
    // Single server-side source of truth for the username format rule.
    // Must match the client-side 'validUsername' rule in _ValidationScriptsPartial.cshtml.
    public static class UsernameValidator
    {
        public const string ErrorMessage =
            "Username may only contain lowercase letters (a-z) - no numbers, spaces, or special characters.";

        public static bool IsValid(string? username)
        {
            return !string.IsNullOrWhiteSpace(username) && Regex.IsMatch(username, "^[a-z]+$");
        }
    }
}
