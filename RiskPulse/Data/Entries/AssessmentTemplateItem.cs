using System.ComponentModel.DataAnnotations;
using RiskPulse.Models.Enum;

namespace RiskPulse.Data.Entries
{
    public class AssessmentTemplateItem
    {
        [Key]
        public int AssessmentTemplateItemId { get; set; }

        public int AssessmentTemplateId { get; set; }

        public AssessmentTemplate AssessmentTemplate { get; set; } = null!;

        public AssessmentItemType ItemType { get; set; }

        public int ItemId { get; set; }
    }
}
