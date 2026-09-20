using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class AssessmentUnit
    {
        [Key]
        public int AssessmentUnitId { get; set; }

        public int AssessmentHeaderId { get; set; }

        public AssessmentHeader? AssessmentHeader { get; set; }

        public int UnitId { get; set; }

        public Unit? Unit { get; set; }

        public int WorkflowStepId { get; set; }

        public WorkflowStep? WorkflowStep { get; set; }

        public int? UnitApprovedById { get; set; }

        public User? UnitApprovedBy { get; set; }

        public DateTime? UnitApprovedOn { get; set; }

        public int? RiskReviewedById { get; set; }

        public User? RiskReviewedBy { get; set; }

        public DateTime? RiskReviewedOn { get; set; }

        public string? RiskReviewerRemarks { get; set; }

        public int? FinalApprovedById { get; set; }

        public User? FinalApprovedBy { get; set; }

        public DateTime? FinalApprovedOn { get; set; }

        public string? FinalApproverRemarks { get; set; }

        public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
    }
}