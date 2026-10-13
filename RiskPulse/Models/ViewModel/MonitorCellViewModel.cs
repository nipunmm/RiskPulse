namespace RiskPulse.Models.ViewModel
{
    public class MonitorCellViewModel
    {
        public int AssessmentUnitId { get; set; }

        public string StepCode { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public string StatusKind { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        public int SubmittedCount { get; set; }

        public bool IsOverdue { get; set; }
    }
}
