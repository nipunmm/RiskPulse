namespace RiskPulse.Models.ViewModel
{
    public class UserGridRowViewModel
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public int UnitId { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}