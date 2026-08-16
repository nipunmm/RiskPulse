using RiskPulse.Data.Entries;

namespace RiskPulse.Models.ViewModel
{
    public class KriTemplatesIndexViewModel
    {
        public List<KriStatusOptionViewModel> KriStatuses { get; set; } = new List<KriStatusOptionViewModel>();

        public List<KriGroupOptionViewModel> KriGroups { get; set; } = new List<KriGroupOptionViewModel>();

        public List<KriThresholdColor> Colors { get; set; } = new List<KriThresholdColor>();
    }
}
