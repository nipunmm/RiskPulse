using RiskPulse.Models.Enum;

namespace RiskPulse.Models.ViewModel
{
    public class ScheduleWizardViewModel
    {
        public int ScheduleId { get; set; }

        public string ScheduleCode { get; set; } = string.Empty;

        public ScheduleType ScheduleType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartMonth { get; set; }

        public int? RecurringDay { get; set; }

        public List<int> SaqHeaderIds { get; set; } = new();

        public List<int> KriHeaderIds { get; set; } = new();

        public List<OptionViewModel> SaqOptions { get; set; } = new();

        public List<OptionViewModel> KriOptions { get; set; } = new();

        public bool CompletedSaq { get; set; }

        public bool CompletedKri { get; set; }

        public bool CompletedSchedule { get; set; }

        public bool CanEdit { get; set; }
    }
}