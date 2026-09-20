using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Data.Entries
{
    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }

        public string ScheduleCode { get; set; } = string.Empty;

        public ScheduleStatus ScheduleStatus { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartMonth { get; set; }

        public int? RecurringDay { get; set; }

        public ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();

        public int? RiskRegisterHeaderId { get; set; }
    }
}