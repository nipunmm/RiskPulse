using RiskPulse.Models.Enum;

namespace RiskPulse.Models.ViewModel
{
    public class MonitorIndexViewModel
    {
        public List<MonitorAssessmentColumnViewModel> Assessments { get; set; } = new();

        public List<OptionViewModel> AssessmentOptions { get; set; } = new();

        public List<MonitorUnitRowViewModel> Units { get; set; } = new();

        public MonitorSummaryViewModel Summary { get; set; } = new();

        public List<MonitorStatusSliceViewModel> Slices { get; set; } = new();

        public int? AssessmentHeaderId { get; set; }

        public UnitType? UnitType { get; set; }

        public bool Truncated { get; set; }

        public string? LoadError { get; set; }
    }
}
