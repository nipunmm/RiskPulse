namespace RiskPulse.Models.ViewModel
{
    public class AssessmentActivationGridRowViewModel
    {
        public int AssessmentActivationId { get; set; }

        public string ActivationCode { get; set; } = string.Empty;

        public string AssessmentTemplateDesc { get; set; } = string.Empty;

        public int? AssessmentTemplateId { get; set; }

        public int SaqCount { get; set; }

        public int KriCount { get; set; }

        public string AssessmentType { get; set; } = string.Empty;

        public string ActivationStatus { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartMonth { get; set; }

        public int? RecurringDay { get; set; }
    }
}
