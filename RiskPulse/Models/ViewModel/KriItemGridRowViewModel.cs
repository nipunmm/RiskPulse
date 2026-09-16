namespace RiskPulse.Models.ViewModel
{
    public class KriItemGridRowViewModel
    {
        public int KriId { get; set; }

        public string KriDesc { get; set; } = string.Empty;

        public bool AllowComment { get; set; }

        public int GreenLimit { get; set; }

        public int AmberLimit { get; set; }

        public int RedLimit { get; set; }
    }
}
