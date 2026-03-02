using System.ComponentModel.DataAnnotations;

namespace F1Store.Models.Team
{
    public class TeamPairVM
    {
        public int Id { get; set; }
        [Display(Name = "Team")]
        public string Name { get; set; } = null!;
    }
}