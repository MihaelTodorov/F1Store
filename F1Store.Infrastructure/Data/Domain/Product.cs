using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F1Store.Infrastructure.Data.Domain
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string ProductName { get; set; } = null!;

        [Required]
        public int TeamId { get; set; }

        public virtual Team Team { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; } = null!;

        public string Picture { get; set; } = null!;

        [Range(0, 5000)]
        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Discount { get; set; }

        public virtual IEnumerable<Order> Orders { get; set; } = new List<Order>();
    }
}
