namespace RiskPulse.Models.ViewModel
{
    public class SaqPreviewViewModel
    {
        public int SaqHeaderId { get; set; }

        public string SaqCode { get; set; } = string.Empty;

        public string? SaqDesc { get; set; }

        public string SaqStatus { get; set; } = string.Empty;

        public string AssignmentLabel { get; set; } = string.Empty;

        public List<SaqQuestionGridRowViewModel> Questions { get; set; } = new List<SaqQuestionGridRowViewModel>();
    }
}