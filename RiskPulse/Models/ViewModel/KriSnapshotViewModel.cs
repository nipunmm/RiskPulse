namespace RiskPulse.Models.ViewModel
{
    public class KriSnapshotViewModel
    {
        public string PeriodLabel { get; set; } = string.Empty;

        public int GreenCount { get; set; }

        public int AmberCount { get; set; }

        public int RedCount { get; set; }

        public int EnteredCount { get; set; }

        public int ExpectedCount { get; set; }

        public bool HasData => EnteredCount > 0;
    }
}