using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class SaqTemplateSaveDto
    {
        [Required(ErrorMessage = "Template header is required.")]
        public SaqHeaderSaveDto Header { get; set; } = new SaqHeaderSaveDto();

        public List<SaqQuestionSaveDto> Questions { get; set; } = new List<SaqQuestionSaveDto>();
    }
}