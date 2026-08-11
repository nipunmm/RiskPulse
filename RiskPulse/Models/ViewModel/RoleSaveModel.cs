namespace RiskPulse.Models.ViewModel
{
    public class RoleSaveModel
    {
        public int RoleId { get; set; }

        public string RoleDesc { get; set; } = string.Empty;

        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
