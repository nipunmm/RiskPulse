using RiskPulse.Models.Enum;

namespace RiskPulse.Models.ViewModel
{
    public class MonitorUnitRowViewModel
    {
        public int UnitId { get; set; }

        public string UnitCode { get; set; } = string.Empty;

        public string UnitDesc { get; set; } = string.Empty;

        public UnitType UnitType { get; set; }

        public List<MonitorCellViewModel?> Cells { get; set; } = new();

        public int TotalCount { get; set; }

        public int CompletedCount { get; set; }
    }
}
