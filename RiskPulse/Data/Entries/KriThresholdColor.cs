using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class KriThresholdColor
    {
        [Key]
        public int ColorId { get; set; }

        public string ColorDesc { get; set; } = string.Empty;

        public string HexCode { get; set; } = string.Empty;

        public ICollection<KriThreshold> KriThresholds { get; set; } = new List<KriThreshold>();
    }
}
