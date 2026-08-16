namespace RiskPulse.Models.ViewModel
{
    public class SaqQuestionGridRowViewModel
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public bool AllowComment { get; set; }

        public int DisplayOrder { get; set; }

        public List<SaqOptionGridRowViewModel> Options { get; set; } = new List<SaqOptionGridRowViewModel>();
    }
}
