namespace RiskPulse.Models.ViewModel
{
    public class KriTemplatesIndexViewModel
    {
        public List<KriStatusOptionViewModel> KriStatuses { get; set; } = new List<KriStatusOptionViewModel>();

        public List<OptionViewModel> UnitGroups { get; set; } = new List<OptionViewModel>();

        public List<OptionViewModel> Units { get; set; } = new List<OptionViewModel>();

        public List<OptionViewModel> KriGroups { get; set; } = new List<OptionViewModel>();

        public List<KriColorOptionViewModel> Colors { get; set; } = new List<KriColorOptionViewModel>();
    }
}
