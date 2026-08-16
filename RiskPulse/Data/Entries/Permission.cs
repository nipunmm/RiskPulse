using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        public string PermissionDesc { get; set; } = string.Empty;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
