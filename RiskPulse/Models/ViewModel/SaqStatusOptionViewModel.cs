using RiskPulse.Models.Enum;

namespace RiskPulse.Models.ViewModel
{
    public class SaqStatusOptionViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public static List<SaqStatusOptionViewModel> GetAll()
        {
            return System.Enum.GetValues<SaqStatus>()
                .Where(s => s != SaqStatus.Locked)
                .Select(s => new SaqStatusOptionViewModel { Value = s.ToString(), Label = s.ToString() })
                .ToList();
        }
    }
}
