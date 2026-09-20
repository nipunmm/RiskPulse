namespace RiskPulse.Models.ViewModel
{
    public class DashboardViewModel
    {
        public string UnitDesc { get; set; } = string.Empty;

        public string CurrentPeriodLabel { get; set; } = string.Empty;

        public string LoadError { get; set; } = string.Empty;

        public List<DashboardKpiViewModel> Kpis { get; set; } = new List<DashboardKpiViewModel>();

        public List<DashboardStatusSliceViewModel> StatusSlices { get; set; } = new List<DashboardStatusSliceViewModel>();

        public List<AssessmentProgressRowViewModel> AssessmentProgress { get; set; } = new List<AssessmentProgressRowViewModel>();

        public KriSnapshotViewModel KriSnapshot { get; set; } = new KriSnapshotViewModel();

        public List<DashboardAttentionRowViewModel> AttentionItems { get; set; } = new List<DashboardAttentionRowViewModel>();

        public List<PeriodHistoryRowViewModel> PeriodHistory { get; set; } = new List<PeriodHistoryRowViewModel>();
    }
}