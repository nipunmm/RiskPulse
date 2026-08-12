namespace RiskPulse.Models.ViewModel
{
    public class KriTemplatesIndexViewModel
    {
        public List<KriStatusOption> KriStatuses { get; set; } = new List<KriStatusOption>();

        public List<KriGroupOption> KriGroups { get; set; } = new List<KriGroupOption>();
    }
}
