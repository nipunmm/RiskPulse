using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.Kri
{
    public class KriHeader
    {
        [Key]
        public int KriHeaderId { get; set; }

        public string KriHeaderDesc { get; set; } = string.Empty;

        public KriStatus KriStatus { get; set; }

        public ICollection<Kri> Kris { get; set; } = new List<Kri>();
    }
}
