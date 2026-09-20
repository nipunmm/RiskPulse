namespace RiskPulse.Models.ViewModel
{
    public class AssessmentDetailViewModel
    {
        public int AssessmentUnitId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public string UnitDesc { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public string StatusLabel { get; set; } = string.Empty;

        public bool CanAuthorize { get; set; }

        public int ItemCount { get; set; }

        public int ApprovedCount { get; set; }

        public string AuthorizedBy { get; set; } = string.Empty;

        public DateTime? AuthorizedOn { get; set; }

        public List<AssessmentItemRowViewModel> Items { get; set; } = new List<AssessmentItemRowViewModel>();
    }
}