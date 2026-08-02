using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.AccessControl
{
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        public int RoleId { get; set; }

        public int PermissionId { get; set; }

        public Role? Role { get; set; }

        public Permission? Permission { get; set; }
    }
}
