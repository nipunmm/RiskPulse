namespace RiskPulse.Models.ViewModel
{
    public class SubmissionsIndexViewModel
    {
        public string UnitDesc { get; set; } = string.Empty;

        public List<SubmissionGridRowViewModel> Pending { get; set; } = new();

        public List<SubmissionGridRowViewModel> InProgress { get; set; } = new();

        public List<SubmissionGridRowViewModel> Completed { get; set; } = new();
    }
}
