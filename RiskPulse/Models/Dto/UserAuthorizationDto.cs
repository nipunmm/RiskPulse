using System.Security.Claims;

namespace RiskPulse.Models.Dto
{
    public class UserAuthorizationDto
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string? RoleDesc { get; set; }

        public string? UnitDesc { get; set; }

        public string? DefaultPermissionDesc { get; set; }

        public List<string> PermissionDescs { get; set; } = new();
    }
}
