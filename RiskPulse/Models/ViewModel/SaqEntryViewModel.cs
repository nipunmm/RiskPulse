namespace RiskPulse.Models.ViewModel
{
    public class SaqEntryViewModel
    {
        public int AssessmentItemId { get; set; }

        public int AssessmentUnitId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public string ItemTitle { get; set; } = string.Empty;

        public string UnitDesc { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public bool Editable { get; set; }

        public List<SaqEntryQuestionViewModel> Questions { get; set; } = new List<SaqEntryQuestionViewModel>();
    }
}