using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class UnitAuthorizeDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid assessment is required.")]
        public int AssessmentUnitId { get; set; }
    }
}