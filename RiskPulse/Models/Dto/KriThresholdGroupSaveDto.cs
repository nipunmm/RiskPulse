using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class KriThresholdGroupSaveDto
    {
        public int KriThresholdGroupId { get; set; }

        [Required(ErrorMessage = "Group description is required.")]
        [StringLength(150, ErrorMessage = "Group description cannot exceed 150 characters.")]
        public string KriThresholdGroupDesc { get; set; } = string.Empty;
    }
}
