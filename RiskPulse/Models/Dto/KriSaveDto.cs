using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class KriSaveDto
    {
        public int KriId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid template.")]
        public int KriHeaderId { get; set; }

        [Required(ErrorMessage = "KRI description is required.")]
        public string KriDesc { get; set; } = string.Empty;

        public bool AllowComment { get; set; } = true;

        [Range(0, int.MaxValue, ErrorMessage = "Green limit must be a non-negative number.")]
        public int GreenLimit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Amber limit must be a non-negative number.")]
        public int AmberLimit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Red limit must be a non-negative number.")]
        public int RedLimit { get; set; }
    }
}
