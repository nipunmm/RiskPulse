using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class AssessmentTemplateSaveDto
    {
        public int AssessmentTemplateId { get; set; }

        [StringLength(200, ErrorMessage = "Template description cannot exceed 200 characters.")]
        public string? AssessmentTemplateDesc { get; set; }

        public bool IsActive { get; set; }

        public List<int> SaqHeaderIds { get; set; } = new List<int>();

        public List<int> KriHeaderIds { get; set; } = new List<int>();
    }
}
