namespace RiskPulse.Models.ViewModel
{
    public class AssessmentWizardViewModel
    {
        public int AssessmentHeaderId { get; set; }

        public string AssessmentName { get; set; } = string.Empty;

        public int SaqHeaderId { get; set; }

        public int KriHeaderId { get; set; }

        public string ScheduleDesc { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<OptionViewModel> SaqOptions { get; set; } = new();

        public List<OptionViewModel> KriOptions { get; set; } = new();

        public bool CompletedSaq { get; set; }

        public bool CompletedKri { get; set; }

        public bool CompletedSchedule { get; set; }

        public bool CanEdit { get; set; }
    }
}
