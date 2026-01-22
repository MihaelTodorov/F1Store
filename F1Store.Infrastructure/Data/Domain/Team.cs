using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F1Store.Infrastructure.Data.Domain
{
    public class Team
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string TeamName { get; set; } = null!;

        public virtual IEnumerable<Team> Teams { get; set; } = new List<Product>();
    }
}