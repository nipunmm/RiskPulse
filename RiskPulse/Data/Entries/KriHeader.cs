using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Data.Entries
{
    public class KriHeader
    {
        [Key]
        public int KriHeaderId { get; set; }

        public string KriHeaderDesc { get; set; } = string.Empty;

        public int? GroupId { get; set; }

        public Group? Group { get; set; }

        public int? UnitId { get; set; }

        public Unit? Unit { get; set; }

        public KriStatus KriStatus { get; set; }

        public string? KriCode { get; set; }

        public ICollection<Kri> Kris { get; set; } = new List<Kri>();

        public ICollection<AssessmentHeader> AssessmentHeaders { get; set; } = new List<AssessmentHeader>();
    }
}
