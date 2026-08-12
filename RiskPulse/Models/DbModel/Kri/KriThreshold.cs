using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.Kri
{
    public class KriThreshold
    {
        [Key]
        public int KriThresholdId { get; set; }

        public int KriThresholdGroupId { get; set; }

        public KriThresholdGroup? KriThresholdGroup { get; set; }

        public int ColorId { get; set; }

        public KriThresholdColor? Color { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }
    }
}
