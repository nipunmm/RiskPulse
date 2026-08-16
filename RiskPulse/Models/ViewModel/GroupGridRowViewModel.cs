namespace RiskPulse.Models.ViewModel
{
    public class GroupGridRowViewModel
    {
        public int GroupId { get; set; }

        public string GroupDesc { get; set; } = string.Empty;

        public int UnitCount { get; set; }

        public List<int> UnitIds { get; set; } = new List<int>();

        public List<string> UnitDescs { get; set; } = new List<string>();
    }
}
