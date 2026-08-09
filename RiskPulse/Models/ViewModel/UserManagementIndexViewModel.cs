using RiskPulse.Models.DbModel.AccessControl;

namespace RiskPulse.Models.ViewModel
{
    public class UserManagementIndexViewModel
    {
        public int CurrentUserId { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public List<Unit> Units { get; set; } = new List<Unit>();

        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
