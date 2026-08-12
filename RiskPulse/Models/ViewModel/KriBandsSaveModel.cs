using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.ViewModel
{
    public class KriBandsSaveModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid threshold group.")]
        public int KriThresholdGroupId { get; set; }

        [MinLength(1, ErrorMessage = "At least one band is required.")]
        public List<KriBandSaveModel> Bands { get; set; } = new List<KriBandSaveModel>();
    }
}
