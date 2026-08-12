namespace RiskPulse.Models.ViewModel
{
    public class SaqOptionGridRow
    {
        public int OptionId { get; set; }

        public string OptionText { get; set; } = string.Empty;

        public string? OptionValue { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
