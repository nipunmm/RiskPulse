using RiskPulse.Models.Enum;

namespace RiskPulse.Models.ViewModel
{
    public class KriStatusOptionViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public static List<KriStatusOptionViewModel> GetAll()
        {
            return System.Enum.GetValues<KriStatus>()
                .Where(s => s != KriStatus.Locked)
                .Select(s => new KriStatusOptionViewModel { Value = s.ToString(), Label = s.ToString() })
                .ToList();
        }
    }
}
