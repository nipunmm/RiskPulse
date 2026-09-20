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

        public int? AuthorizedById { get; set; }

        public User? AuthorizedBy { get; set; }

        public DateTime? AuthorizedOn { get; set; }

        public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
    }
}