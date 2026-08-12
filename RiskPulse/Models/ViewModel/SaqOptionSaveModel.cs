using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class SaqOptionSaveModel
    {
        public int OptionId { get; set; }

        [Required(ErrorMessage = "Option text is required.")]
        [StringLength(300, ErrorMessage = "Option text cannot exceed 300 characters.")]
        public string OptionText { get; set; } = string.Empty;
    }
}
