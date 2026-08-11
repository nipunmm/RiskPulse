namespace RiskPulse.Models.ViewModel
{
    public class RoleGridRow
    {
        public int RoleId { get; set; }

        public string RoleDesc { get; set; } = string.Empty;

        public int? DefaultPermissionId { get; set; }

        public string? DefaultPermissionDesc { get; set; }

        public List<int> PermissionIds { get; set; } = new List<int>();

        public List<string?> PermissionDescs { get; set; } = new List<string?>();
    }
}