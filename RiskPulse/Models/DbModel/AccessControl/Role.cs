using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.AccessControl
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleDesc { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
