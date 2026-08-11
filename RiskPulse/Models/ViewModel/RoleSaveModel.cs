using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class RoleSaveModel
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters.")]
        public string RoleDesc { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one permission is required.")]
        [MinLength(1, ErrorMessage = "At least one permission is required.")]
        public List<int> PermissionIds { get; set; } = new List<int>();

        public int? DefaultPermissionId { get; set; }
    }
}
