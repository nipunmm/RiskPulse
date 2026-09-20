namespace RiskPulse.Models.ViewModel
{
    public class SubmissionGridRowViewModel
    {
        public int AssessmentUnitId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public string UnitDesc { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public string StatusLabel { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        public int SubmittedCount { get; set; }
    }
}