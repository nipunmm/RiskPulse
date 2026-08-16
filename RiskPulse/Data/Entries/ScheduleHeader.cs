using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class ScheduleHeader
    {
        [Key]
        public int ScheduleHeaderId { get; set; }

        public int AssessmentHeaderId { get; set; }

        public AssessmentHeader? AssessmentHeader { get; set; }

        public string ScheduleDesc { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
