using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class KriAssessmentValue
    {
        [Key]
        public int KriAssessmentValueId { get; set; }

        public int AssessmentItemId { get; set; }

        public AssessmentItem? AssessmentItem { get; set; }

        public int KriId { get; set; }

        public Kri? Kri { get; set; }

        public decimal Value { get; set; }

        public string? Comment { get; set; }
    }
}