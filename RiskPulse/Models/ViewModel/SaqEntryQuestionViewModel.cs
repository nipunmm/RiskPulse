namespace RiskPulse.Models.ViewModel
{
    public class SaqEntryQuestionViewModel
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public bool AllowComment { get; set; }

        public int DisplayOrder { get; set; }

        public int? SelectedOptionId { get; set; }

        public string? Comment { get; set; }

        public List<SaqOptionGridRowViewModel> Options { get; set; } = new List<SaqOptionGridRowViewModel>();
    }
}