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

        public string StepCode { get; set; } = string.Empty;

        public string CurrentUserRole { get; set; } = string.Empty;

        public bool CanUnitApprove { get; set; }

        public bool CanRiskReview { get; set; }

        public bool CanFinalApprove { get; set; }

        public bool CanReturn { get; set; }

        public int ItemCount { get; set; }

        public int ApprovedCount { get; set; }

        public string? UnitApprovedBy { get; set; }

        public DateTime? UnitApprovedOn { get; set; }

        public string? RiskReviewedBy { get; set; }

        public DateTime? RiskReviewedOn { get; set; }

        public string? RiskReviewerRemarks { get; set; }

        public string? FinalApprovedBy { get; set; }

        public DateTime? FinalApprovedOn { get; set; }

        public string? FinalApproverRemarks { get; set; }

        public List<AssessmentItemRowViewModel> Items { get; set; } = new List<AssessmentItemRowViewModel>();
    }
}