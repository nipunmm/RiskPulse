using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class AssessmentTemplate
    {
        [Key]
        public int AssessmentTemplateId { get; set; }

        public string AssessmentTemplateDesc { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string? AssessmentTemplateCode { get; set; }

        public ICollection<AssessmentTemplateItem> AssessmentTemplateItems { get; set; } = new List<AssessmentTemplateItem>();
    }
}
