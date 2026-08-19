using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class SaqHeader
    {
        [Key]
        public int SaqHeaderId { get; set; }

        public string SaqDesc { get; set; } = string.Empty;

        public int? GroupId { get; set; }

        public Group? Group { get; set; }

        public int? UnitId { get; set; }

        public Unit? Unit { get; set; }

        public SaqStatus SaqStatus { get; set; }

        public ICollection<SaqQuestion> SaqQuestions { get; set; } = new List<SaqQuestion>();

        public ICollection<AssessmentHeader> AssessmentHeaders { get; set; } = new List<AssessmentHeader>();
    }
}
