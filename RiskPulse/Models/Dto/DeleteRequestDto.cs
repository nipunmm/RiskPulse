using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class DeleteRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid ID is required.")]
        public int Id { get; set; }
    }
}
