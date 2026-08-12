using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class KriBandSaveModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid color.")]
        public int ColorId { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }
    }
}
