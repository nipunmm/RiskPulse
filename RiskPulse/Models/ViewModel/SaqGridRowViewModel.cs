namespace RiskPulse.Models.ViewModel
{
    public class SaqGridRowViewModel
    {
        public int SaqHeaderId { get; set; }

        public string SaqDesc { get; set; } = string.Empty;

        public int? GroupId { get; set; }

        public string GroupDesc { get; set; } = string.Empty;

        public int? UnitId { get; set; }

        public string UnitDesc { get; set; } = string.Empty;

        public string SaqStatus { get; set; } = string.Empty;

        public int QuestionCount { get; set; }
    }
}
