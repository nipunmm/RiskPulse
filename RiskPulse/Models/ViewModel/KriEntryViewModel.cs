namespace RiskPulse.Models.ViewModel
{
    public class KriEntryViewModel
    {
        public int AssessmentItemId { get; set; }

        public int AssessmentUnitId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public string ItemTitle { get; set; } = string.Empty;

        public string UnitDesc { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public bool Editable { get; set; }

        public List<KriEntryValueViewModel> Values { get; set; } = new List<KriEntryValueViewModel>();
    }
}