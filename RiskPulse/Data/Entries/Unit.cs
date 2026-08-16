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
    }
}
