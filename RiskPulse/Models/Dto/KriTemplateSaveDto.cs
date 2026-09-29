using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class KriTemplateSaveDto
    {
        [Required(ErrorMessage = "Template header is required.")]
        public KriHeaderSaveDto Header { get; set; } = new KriHeaderSaveDto();

        public List<KriSaveDto> Kris { get; set; } = new List<KriSaveDto>();
    }
}