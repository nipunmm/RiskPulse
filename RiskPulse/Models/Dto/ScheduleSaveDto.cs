using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class ScheduleSaveDto
    {
        public int AssessmentHeaderId { get; set; }

        [Required(ErrorMessage = "Schedule description is required.")]
        [StringLength(200, ErrorMessage = "Schedule description cannot exceed 200 characters.")]
        public string ScheduleDesc { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
