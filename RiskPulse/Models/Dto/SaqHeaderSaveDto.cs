using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Models.Dto
{
    public class SaqHeaderSaveDto
    {
        public int SaqHeaderId { get; set; }

        public int? GroupId { get; set; }

        public int? UnitId { get; set; }

        [Required(ErrorMessage = "Template description is required.")]
        [StringLength(200, ErrorMessage = "Template description cannot exceed 200 characters.")]
        public string SaqDesc { get; set; } = string.Empty;

        public SaqStatus SaqStatus { get; set; }
    }
}
