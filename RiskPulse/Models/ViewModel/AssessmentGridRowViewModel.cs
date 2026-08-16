namespace RiskPulse.Models.ViewModel
{
    public class AssessmentGridRowViewModel
    {
        public int AssessmentHeaderId { get; set; }

        public string AssessmentName { get; set; } = string.Empty;

        public string SaqDesc { get; set; } = string.Empty;

        public string KriHeaderDesc { get; set; } = string.Empty;

        public string ScheduleDesc { get; set; } = string.Empty;

        public string AssessmentStatus { get; set; } = string.Empty;
    }
}
