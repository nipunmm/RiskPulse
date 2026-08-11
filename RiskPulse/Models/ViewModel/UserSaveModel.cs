using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class UserSaveModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression("^[a-z]+$", ErrorMessage = "Username must be lowercase letters only (a-z), no numbers, spaces, or special characters.")]
        public string Username { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit.")]
        public int UnitId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a role.")]
        public int RoleId { get; set; }
    }
}