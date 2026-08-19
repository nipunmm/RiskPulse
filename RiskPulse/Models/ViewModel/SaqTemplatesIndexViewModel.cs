namespace RiskPulse.Models.ViewModel
{
    public class SaqTemplatesIndexViewModel
    {
        public List<SaqStatusOptionViewModel> SaqStatuses { get; set; } = new List<SaqStatusOptionViewModel>();

        public List<UnitGroupOptionViewModel> UnitGroups { get; set; } = new List<UnitGroupOptionViewModel>();

        public List<UnitGroupOptionViewModel> Units { get; set; } = new List<UnitGroupOptionViewModel>();
    }
}
