using RiskPulse.Models.DbModel.AccessControl;

namespace RiskPulse.Models.ViewModel
{
    public class RolesIndexViewModel
    {
        public List<Role> Roles { get; set; } = new List<Role>();

        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
