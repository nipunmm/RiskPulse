using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class SaveKriValuesDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid assessment item is required.")]
        public int AssessmentItemId { get; set; }

        public List<KriValueSaveDto> Values { get; set; } = new List<KriValueSaveDto>();
    }

    public class KriValueSaveDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid KRI is required.")]
        public int KriId { get; set; }

        public decimal? Value { get; set; }

        public string? Comment { get; set; }
    }
}