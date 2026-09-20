namespace RiskPulse.Models.ViewModel
{
    public class ScheduleGridRowViewModel
    {
        public int ScheduleId { get; set; }

        public string ScheduleCode { get; set; } = string.Empty;

        public int SaqCount { get; set; }

        public List<string> SaqCodes { get; set; } = new();

        public int KriCount { get; set; }

        public List<string> KriCodes { get; set; } = new();

        public string ScheduleType { get; set; } = string.Empty;

        public string ScheduleStatus { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartMonth { get; set; }

        public int? RecurringDay { get; set; }
    }
}