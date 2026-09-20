using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class Workflow
    {
        [Key]
        public int WorkflowId { get; set; }

        public string WorkflowCode { get; set; } = string.Empty;

        public string WorkflowName { get; set; } = string.Empty;

        public ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
    }
}