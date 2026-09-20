namespace RiskPulse.Models.ViewModel
{
    public class AssessmentItemRowViewModel
    {
        public int AssessmentItemId { get; set; }

        public string ItemType { get; set; } = string.Empty;

        public string ItemTitle { get; set; } = string.Empty;

        public string ReferenceCode { get; set; } = string.Empty;

        public string StepCode { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public bool Editable { get; set; }

        public string SubmittedBy { get; set; } = string.Empty;

        public DateTime? SubmittedOn { get; set; }

        public string ApprovedBy { get; set; } = string.Empty;

        public DateTime? ApprovedOn { get; set; }
    }
}