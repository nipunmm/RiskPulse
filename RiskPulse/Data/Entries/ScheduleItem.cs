using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Data.Entries
{
    public class ScheduleItem
    {
        [Key]
        public int ScheduleItemId { get; set; }

        public int ScheduleId { get; set; }

        public Schedule Schedule { get; set; } = null!;

        public ScheduleItemType ItemType { get; set; }

        public int ItemId { get; set; }
    }
}