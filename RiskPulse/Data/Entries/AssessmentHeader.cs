using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class AssessmentHeader
    {
        [Key]
        public int AssessmentHeaderId { get; set; }

        public string AssessmentName { get; set; } = string.Empty;

        public AssessmentStatus AssessmentStatus { get; set; }

        public int? SaqHeaderId { get; set; }

        public SaqHeader? SaqHeader { get; set; }

        public int? KriHeaderId { get; set; }

        public KriHeader? KriHeader { get; set; }

        public int? RiskRegisterHeaderId { get; set; }

        public ICollection<ScheduleHeader> ScheduleHeaders { get; set; } = new List<ScheduleHeader>();
    }
}
