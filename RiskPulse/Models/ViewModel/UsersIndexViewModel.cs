using RiskPulse.Data.Entries;

namespace RiskPulse.Models.ViewModel
{
    public class UsersIndexViewModel
    {
        public int CurrentUserId { get; set; }

        public List<Unit> Units { get; set; } = new List<Unit>();

        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
