using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Models.Dto
{
    public class AssessmentActivationSaveDto
    {
        public int AssessmentActivationId { get; set; }

        public AssessmentType AssessmentType { get; set; }

        public int? AssessmentTemplateId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartMonth { get; set; }

        [Range(1, 31, ErrorMessage = "Recurring day must be between 1 and 31.")]
        public int? RecurringDay { get; set; }
    }
}
