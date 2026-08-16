using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class KriColorSaveDto
    {
        public int ColorId { get; set; }

        [Required(ErrorMessage = "Color description is required.")]
        [StringLength(100, ErrorMessage = "Color description cannot exceed 100 characters.")]
        public string ColorDesc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hex code is required.")]
        [RegularExpression("^#([0-9A-Fa-f]{6})$", ErrorMessage = "Hex code must be in #RRGGBB format.")]
        public string HexCode { get; set; } = string.Empty;
    }
}
