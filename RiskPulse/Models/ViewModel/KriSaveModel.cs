using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class KriSaveModel
    {
        public int KriId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid template.")]
        public int KriHeaderId { get; set; }

        [Required(ErrorMessage = "KRI description is required.")]
        public string KriDesc { get; set; } = string.Empty;

        public bool AllowComment { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a threshold group.")]
        public int KriThresholdGroupId { get; set; }
    }
}
