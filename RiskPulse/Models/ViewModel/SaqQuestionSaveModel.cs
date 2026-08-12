using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class SaqQuestionSaveModel
    {
        public int QuestionId { get; set; }

        public int SaqHeaderId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        public string QuestionText { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public int DisplayOrder { get; set; }

        [MinLength(1, ErrorMessage = "At least one option is required.")]
        public List<SaqOptionSaveModel> Options { get; set; } = new List<SaqOptionSaveModel>();
    }
}
