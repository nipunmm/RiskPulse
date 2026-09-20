using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class ItemTransitionDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid assessment item is required.")]
        public int AssessmentItemId { get; set; }
    }
}