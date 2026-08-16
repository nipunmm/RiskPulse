using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class Kri
    {
        [Key]
        public int KriId { get; set; }

        public int KriHeaderId { get; set; }

        public KriHeader? KriHeader { get; set; }

        public string KriDesc { get; set; } = string.Empty;

        public bool AllowComment { get; set; } = true;

        public int KriThresholdGroupId { get; set; }

        public KriThresholdGroup? KriThresholdGroup { get; set; }
    }
}
