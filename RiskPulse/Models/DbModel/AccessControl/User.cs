using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.AccessControl
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public int UnitId { get; set; }

        public int RoleId { get; set; }

        public Unit? Unit { get; set; }

        public Role? Role { get; set; }
    }
}
