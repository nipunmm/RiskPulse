using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Data.Entries
{
    public class AssessmentActivation
    {
        [Key]
        public int AssessmentActivationId { get; set; }

        public string ActivationCode { get; set; } = string.Empty;

        public AssessmentActivationStatus ActivationStatus { get; set; }

        public AssessmentType AssessmentType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartMonth { get; set; }

        public int? RecurringDay { get; set; }

        public int? AssessmentTemplateId { get; set; }

        public AssessmentTemplate? AssessmentTemplate { get; set; }

        public ICollection<AssessmentHeader> AssessmentHeaders { get; set; } = new List<AssessmentHeader>();

        public int? RiskRegisterHeaderId { get; set; }
    }
}
