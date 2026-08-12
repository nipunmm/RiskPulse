namespace RiskPulse.Models.ViewModel
{
    public class KriItemGridRow
    {
        public int KriId { get; set; }

        public string KriDesc { get; set; } = string.Empty;

        public bool AllowComment { get; set; }

        public int KriThresholdGroupId { get; set; }

        public string? KriThresholdGroupDesc { get; set; }
    }
}
