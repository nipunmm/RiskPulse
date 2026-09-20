using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class SaveSaqAnswersDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid assessment item is required.")]
        public int AssessmentItemId { get; set; }

        public List<SaqAnswerSaveDto> Answers { get; set; } = new List<SaqAnswerSaveDto>();
    }

    public class SaqAnswerSaveDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid question is required.")]
        public int QuestionId { get; set; }

        public int OptionId { get; set; }

        public string? Comment { get; set; }
    }
}