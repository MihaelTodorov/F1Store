using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace F1Store.Infrastructure.Data.Domain
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Address { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
    }
}