using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class SaqAssessmentAnswer
    {
        [Key]
        public int SaqAssessmentAnswerId { get; set; }

        public int AssessmentItemId { get; set; }

        public AssessmentItem? AssessmentItem { get; set; }

        public int QuestionId { get; set; }

        public SaqQuestion? Question { get; set; }

        public int OptionId { get; set; }

        public SaqQuestionOption? Option { get; set; }

        public string? Comment { get; set; }
    }
}