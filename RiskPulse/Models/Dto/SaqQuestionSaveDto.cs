using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class SaqQuestionSaveDto
    {
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        public string QuestionText { get; set; } = string.Empty;

        public bool AllowComment { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one option is required.")]
        public List<SaqOptionSaveDto> Options { get; set; } = new List<SaqOptionSaveDto>();
    }
}