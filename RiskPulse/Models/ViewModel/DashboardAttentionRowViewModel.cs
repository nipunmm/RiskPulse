namespace RiskPulse.Models.ViewModel
{
    public class DashboardAttentionRowViewModel
    {
        public int AssessmentUnitId { get; set; }

        public int AssessmentItemId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public string ItemTitle { get; set; } = string.Empty;

        public string ReferenceCode { get; set; } = string.Empty;

        public string ItemType { get; set; } = string.Empty;

        public string ActionKind { get; set; } = string.Empty;

        public string ActionLabel { get; set; } = string.Empty;

        public string StepLabel { get; set; } = string.Empty;

        public DateTime? PeriodEnd { get; set; }

        public bool IsOverdue { get; set; }
    }
}