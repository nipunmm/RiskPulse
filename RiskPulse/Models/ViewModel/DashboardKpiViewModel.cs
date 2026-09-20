namespace RiskPulse.Models.ViewModel
{
    public class DashboardKpiViewModel
    {
        public string Icon { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public int Value { get; set; }

        public string DotKind { get; set; } = string.Empty;

        public string PillText { get; set; } = string.Empty;

        public string PillKind { get; set; } = string.Empty;

        public string Href { get; set; } = string.Empty;
    }
}