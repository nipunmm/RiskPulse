using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.DbModel.Kri;

namespace RiskPulse.Models.ViewModel
{
    public class KriHeaderSaveModel
    {
        public int KriHeaderId { get; set; }

        [Required(ErrorMessage = "Template description is required.")]
        [StringLength(200, ErrorMessage = "Template description cannot exceed 200 characters.")]
        public string KriHeaderDesc { get; set; } = string.Empty;

        public KriStatus KriStatus { get; set; }
    }
}
