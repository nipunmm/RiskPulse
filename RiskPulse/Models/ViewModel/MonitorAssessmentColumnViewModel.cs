namespace RiskPulse.Models.ViewModel
{
    public class MonitorAssessmentColumnViewModel
    {
        public int AssessmentHeaderId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public int UnitCount { get; set; }

        public int CompletedCount { get; set; }
    }
}
