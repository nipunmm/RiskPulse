using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class KriThresholdGroup
    {
        [Key]
        public int KriThresholdGroupId { get; set; }

        public string KriThresholdGroupDesc { get; set; } = string.Empty;

        public ICollection<KriThreshold> KriThresholds { get; set; } = new List<KriThreshold>();

        public ICollection<Kri> Kris { get; set; } = new List<Kri>();
    }
}
