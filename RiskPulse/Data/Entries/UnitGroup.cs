using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Data.Entries
{
    public class UnitGroup
    {
        [Key]
        public int UnitGroupId { get; set; }

        public int GroupId { get; set; }

        public int UnitId { get; set; }

        public Group? Group { get; set; }

        public Unit? Unit { get; set; }
    }
}
