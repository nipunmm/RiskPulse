using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class WorkflowStep
    {
        [Key]
        public int WorkflowStepId { get; set; }

        public int WorkflowId { get; set; }

        public Workflow? Workflow { get; set; }

        public string StepCode { get; set; } = string.Empty;

        public string StepLabel { get; set; } = string.Empty;

        public int StepOrder { get; set; }

        public bool IsInitial { get; set; }

        public bool IsFinal { get; set; }
    }
}