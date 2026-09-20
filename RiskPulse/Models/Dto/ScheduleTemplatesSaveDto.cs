using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class ScheduleTemplatesSaveDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A saved schedule is required.")]
        public int ScheduleId { get; set; }

        [MinLength(1, ErrorMessage = "Select at least one template.")]
        public List<int> TemplateHeaderIds { get; set; } = new();
    }
}