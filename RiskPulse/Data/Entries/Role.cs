using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleDesc { get; set; } = string.Empty;

        public int? DefaultPermissionId { get; set; }

        public Permission? DefaultPermission { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
