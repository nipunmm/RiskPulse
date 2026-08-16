using System.ComponentModel.DataAnnotations;
using RiskPulse.Data.Entries;

namespace RiskPulse.Models.Dto
{
    public class KriHeaderSaveDto
    {
        public int KriHeaderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit group.")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Template description is required.")]
        [StringLength(200, ErrorMessage = "Template description cannot exceed 200 characters.")]
        public string KriHeaderDesc { get; set; } = string.Empty;

        public KriStatus KriStatus { get; set; }
    }
}
