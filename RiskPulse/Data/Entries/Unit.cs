using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        public string UnitCode { get; set; } = string.Empty;

        public UnitType UnitType { get; set; }

        public string UnitDesc { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();

        public ICollection<UnitGroup> UnitGroups { get; set; } = new List<UnitGroup>();

        public ICollection<SaqHeader> SaqHeaders { get; set; } = new List<SaqHeader>();

        public ICollection<KriHeader> KriHeaders { get; set; } = new List<KriHeader>();
    }
}
