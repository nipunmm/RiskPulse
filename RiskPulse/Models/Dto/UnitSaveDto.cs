using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Models.Dto
{
    public class UnitSaveDto
    {
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Unit code is required.")]
        [StringLength(20, ErrorMessage = "Unit code cannot exceed 20 characters.")]
        public string UnitCode { get; set; } = string.Empty;

        public UnitType UnitType { get; set; }

        [Required(ErrorMessage = "Unit description is required.")]
        [StringLength(200, ErrorMessage = "Unit description cannot exceed 200 characters.")]
        public string UnitDesc { get; set; } = string.Empty;
    }
}
