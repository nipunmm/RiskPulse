namespace RiskPulse.Models.ViewModel
{
    public class SaqTemplatesIndexViewModel
    {
        public List<SaqStatusOptionViewModel> SaqStatuses { get; set; } = new List<SaqStatusOptionViewModel>();

        public List<OptionViewModel> UnitGroups { get; set; } = new List<OptionViewModel>();

        public List<OptionViewModel> Units { get; set; } = new List<OptionViewModel>();
    }
}
