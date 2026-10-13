namespace RiskPulse.Models.ViewModel
{
    public class AssessmentTemplateGridRowViewModel
    {
        public int AssessmentTemplateId { get; set; }

        public string AssessmentTemplateCode { get; set; } = string.Empty;

        public string AssessmentTemplateDesc { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int SaqCount { get; set; }

        public int KriCount { get; set; }

        public List<int> SaqHeaderIds { get; set; } = new List<int>();

        public List<int> KriHeaderIds { get; set; } = new List<int>();

        public List<string> SaqCodes { get; set; } = new List<string>();

        public List<string> KriCodes { get; set; } = new List<string>();
    }
}
