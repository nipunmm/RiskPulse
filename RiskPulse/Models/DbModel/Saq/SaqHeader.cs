using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.Saq
{
    public class SaqHeader
    {
        [Key]
        public int SaqHeaderId { get; set; }

        public string SaqDesc { get; set; } = string.Empty;

        public SaqStatus SaqStatus { get; set; }

        public ICollection<SaqQuestion> SaqQuestions { get; set; } = new List<SaqQuestion>();
    }
}
