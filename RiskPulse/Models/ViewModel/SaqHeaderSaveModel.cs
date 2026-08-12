using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.DbModel.Saq;

namespace RiskPulse.Models.ViewModel
{
    public class SaqHeaderSaveModel
    {
        public int SaqHeaderId { get; set; }

        [Required(ErrorMessage = "Template description is required.")]
        [StringLength(200, ErrorMessage = "Template description cannot exceed 200 characters.")]
        public string SaqDesc { get; set; } = string.Empty;

        public SaqStatus SaqStatus { get; set; }
    }
}
