namespace RiskPulse.Models.ViewModel
{
    public class SaqQuestionGridRow
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public bool AllowComment { get; set; }

        public int DisplayOrder { get; set; }

        public List<SaqOptionGridRow> Options { get; set; } = new List<SaqOptionGridRow>();
    }
}
