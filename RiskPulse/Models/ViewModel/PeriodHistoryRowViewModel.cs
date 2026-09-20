namespace RiskPulse.Models.ViewModel
{
    public class PeriodHistoryRowViewModel
    {
        public int AssessmentUnitId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public string StatusLabel { get; set; } = string.Empty;

        public string StatusKind { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        public int SubmittedCount { get; set; }

        public int ApprovedCount { get; set; }

        public bool ReadyToAuthorize { get; set; }

        public string AuthorizedBy { get; set; } = string.Empty;

        public DateTime? AuthorizedOn { get; set; }
    }
}