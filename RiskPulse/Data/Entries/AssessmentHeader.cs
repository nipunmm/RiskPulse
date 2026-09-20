using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Data.Entries
{
    public class AssessmentHeader
    {
        [Key]
        public int AssessmentHeaderId { get; set; }

        public int ScheduleId { get; set; }

        public Schedule? Schedule { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public AssessmentStatus AssessmentStatus { get; set; }

        public ICollection<AssessmentUnit> AssessmentUnits { get; set; } = new List<AssessmentUnit>();
    }
}