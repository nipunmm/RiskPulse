using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class RoleSaveModel
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be at least 2 characters.")]
        public string RoleDesc { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one permission is required.")]
        [MinLength(1, ErrorMessage = "At least one permission is required.")]
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
