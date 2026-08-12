using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.Saq
{
    public class SaqQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        public int SaqHeaderId { get; set; }

        public SaqHeader? SaqHeader { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public QuestionType QuestionType { get; set; }

        public bool AllowComment { get; set; } = true;

        public int DisplayOrder { get; set; }

        public ICollection<SaqQuestionOption> SaqQuestionOptions { get; set; } = new List<SaqQuestionOption>();
    }
}
