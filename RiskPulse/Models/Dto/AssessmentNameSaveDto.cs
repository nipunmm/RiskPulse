using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class AssessmentNameSaveDto
    {
        public int AssessmentHeaderId { get; set; }

        [Required(ErrorMessage = "Assessment name is required.")]
        [StringLength(200, ErrorMessage = "Assessment name cannot exceed 200 characters.")]
        public string AssessmentName { get; set; } = string.Empty;
    }
}
