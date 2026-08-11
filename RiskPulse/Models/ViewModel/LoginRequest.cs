using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression("^[a-z]+$", ErrorMessage = "Username must be lowercase letters only (a-z), no numbers, spaces, or special characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}