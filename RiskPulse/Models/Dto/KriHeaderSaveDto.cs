using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Models.Dto
{
    public class KriHeaderSaveDto
    {
        public int KriHeaderId { get; set; }

        public int? GroupId { get; set; }

        public int? UnitId { get; set; }

        [StringLength(200, ErrorMessage = "Template description cannot exceed 200 characters.")]
        public string KriHeaderDesc { get; set; } = string.Empty;

        public KriStatus KriStatus { get; set; }
    }
}
