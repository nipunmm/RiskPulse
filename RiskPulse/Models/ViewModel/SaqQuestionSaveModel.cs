using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class SaqQuestionSaveModel
    {
        public int QuestionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid template.")]
        public int SaqHeaderId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        public string QuestionText { get; set; } = string.Empty;

        public bool AllowComment { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one option is required.")]
        public List<SaqOptionSaveModel> Options { get; set; } = new List<SaqOptionSaveModel>();
    }
}
