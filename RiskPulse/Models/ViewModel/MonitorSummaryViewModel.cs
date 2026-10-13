namespace RiskPulse.Models.ViewModel
{
    public class MonitorSummaryViewModel
    {
        public int UnitCount { get; set; }

        public int AssessmentCount { get; set; }

        public int TargetCount { get; set; }

        public int CompletedCount { get; set; }

        public int InProgressCount { get; set; }

        public int NotStartedCount { get; set; }

        public int ReturnedCount { get; set; }

        public int OverdueCount { get; set; }
    }
}
