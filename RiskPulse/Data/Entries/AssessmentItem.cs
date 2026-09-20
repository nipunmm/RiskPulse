using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Data.Entries
{
    public class AssessmentItem
    {
        [Key]
        public int AssessmentItemId { get; set; }

        public int AssessmentUnitId { get; set; }

        public AssessmentUnit? AssessmentUnit { get; set; }

        public ScheduleItemType ItemType { get; set; }

        public int ItemId { get; set; }

        public int WorkflowStepId { get; set; }

        public WorkflowStep? WorkflowStep { get; set; }

        public int? SubmittedById { get; set; }

        public User? SubmittedBy { get; set; }

        public int? ApprovedById { get; set; }

        public User? ApprovedBy { get; set; }

        public DateTime? SubmittedOn { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public ICollection<SaqAssessmentAnswer> SaqAssessmentAnswers { get; set; } = new List<SaqAssessmentAnswer>();

        public ICollection<KriAssessmentValue> KriAssessmentValues { get; set; } = new List<KriAssessmentValue>();
    }
}