namespace RiskPulse.Services.Assessment;

// --- Workflow + step-code catalog (must match the WorkflowSteps rows seeded in Data/Seed.sql) ---
public static class WorkflowStepCodes
{
    public const string AssessmentItemWorkflow = "ASSESSMENT-ITEM";
    public const string AssessmentUnitWorkflow = "ASSESSMENT-UNIT";

    // Item-level steps
    public const string Pending = "Pending";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";

    // Unit-level steps
    public const string InProgress = "InProgress";
    public const string UnitApproved = "UnitApproved";
    public const string RiskReviewed = "RiskReviewed";
    public const string FinalApproved = "FinalApproved";
    public const string Returned = "Returned";
}
