using RiskPulse.Models.Enum;

namespace RiskPulse.Models.Dto
{
    public class ScheduleFinalizeDto
    {
        public int ScheduleId { get; set; }

        public ScheduleStatus Status { get; set; }
    }
}