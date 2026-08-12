using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.DbModel.Saq
{
    public class SaqQuestionOption
    {
        [Key]
        public int OptionId { get; set; }

        public int QuestionId { get; set; }

        public SaqQuestion? SaqQuestion { get; set; }

        public string OptionText { get; set; } = string.Empty;

        public string? OptionValue { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
