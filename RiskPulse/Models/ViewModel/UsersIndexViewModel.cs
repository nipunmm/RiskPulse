namespace RiskPulse.Models.ViewModel
{
    public class UsersIndexViewModel
    {
        public int CurrentUserId { get; set; }

        public List<OptionViewModel> Units { get; set; } = new List<OptionViewModel>();

        public List<OptionViewModel> Roles { get; set; } = new List<OptionViewModel>();
    }
}
