namespace RiskPulse.Models.ViewModel
{
    public class KriGridRowViewModel
    {
        public int KriHeaderId { get; set; }

        public string KriHeaderDesc { get; set; } = string.Empty;

        public string KriStatus { get; set; } = string.Empty;

        public int KriCount { get; set; }
    }
}
