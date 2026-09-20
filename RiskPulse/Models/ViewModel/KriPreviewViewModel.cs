namespace RiskPulse.Models.ViewModel
{
    public class KriPreviewViewModel
    {
        public int KriHeaderId { get; set; }

        public string KriCode { get; set; } = string.Empty;

        public string? KriHeaderDesc { get; set; }

        public string KriStatus { get; set; } = string.Empty;

        public string AssignmentLabel { get; set; } = string.Empty;

        public List<KriItemGridRowViewModel> Kris { get; set; } = new List<KriItemGridRowViewModel>();
    }
}