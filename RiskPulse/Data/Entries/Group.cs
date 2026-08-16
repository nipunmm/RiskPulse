using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        public string GroupDesc { get; set; } = string.Empty;

        public ICollection<UnitGroup> UnitGroups { get; set; } = new List<UnitGroup>();

        public ICollection<SaqHeader> SaqHeaders { get; set; } = new List<SaqHeader>();

        public ICollection<KriHeader> KriHeaders { get; set; } = new List<KriHeader>();
    }
}
