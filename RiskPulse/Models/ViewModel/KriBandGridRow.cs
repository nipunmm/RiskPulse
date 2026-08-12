namespace RiskPulse.Models.ViewModel
{
    public class KriBandGridRow
    {
        public int KriThresholdId { get; set; }

        public int ColorId { get; set; }

        public string? ColorDesc { get; set; }

        public string? HexCode { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }
    }
}
