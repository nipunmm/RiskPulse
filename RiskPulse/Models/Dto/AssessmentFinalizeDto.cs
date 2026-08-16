using RiskPulse.Data.Entries;

namespace RiskPulse.Models.Dto
{
    public class AssessmentFinalizeDto
    {
        public int AssessmentHeaderId { get; set; }

        public AssessmentStatus Status { get; set; }
    }
}
