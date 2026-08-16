using System.ComponentModel.DataAnnotations;

namespace RiskPulse.Models.Dto
{
    public class GroupSaveDto
    {
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Group description is required.")]
        [StringLength(200, ErrorMessage = "Group description cannot exceed 200 characters.")]
        public string GroupDesc { get; set; } = string.Empty;

        public List<int> UnitIds { get; set; } = new List<int>();
    }
}
