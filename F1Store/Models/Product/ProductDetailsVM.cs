using System.ComponentModel.DataAnnotations;

namespace F1Store.Models.Product
{
    public class ProductDetailsVM
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = null!;

        public int TeamId { get; set; }
        [Display(Name = "Team")]
        public string TeamName { get; set; } = null!;

        public int CategoryId { get; set; }
        [Display(Name = "Category")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Main Picture")]
        public string Picture { get; set; } = null!;
        public string? Picture2 { get; set; }
        public string? Picture3 { get; set; }
        public string? Picture4 { get; set; }
        public string? Picture5 { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Discount %")]
        public decimal Discount { get; set; }
    }
}