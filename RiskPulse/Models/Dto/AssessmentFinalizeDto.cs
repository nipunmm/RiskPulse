using RiskPulse.Models.Enum;

namespace RiskPulse.Models.Dto
{
    public class AssessmentFinalizeDto
    {
        public int AssessmentHeaderId { get; set; }

        public AssessmentStatus Status { get; set; }
    }
}
