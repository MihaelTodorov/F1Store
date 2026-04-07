using System.ComponentModel.DataAnnotations;
using F1Store.Models.Team;
using F1Store.Models.Category;

namespace F1Store.Models.Product
{
    public class ProductEditVM
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = null!;

        [Required]
        [Display(Name = "Team")]
        public int TeamId { get; set; }
        public virtual List<TeamPairVM> Teams { get; set; } = new List<TeamPairVM>();

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual List<CategoryPairVM> Categories { get; set; } = new List<CategoryPairVM>();

        [Required]
        [Display(Name = "Main Picture URL")]
        public string Picture { get; set; } = null!;

        [Display(Name = "Picture 2 URL")]
        public string? Picture2 { get; set; }

        [Display(Name = "Picture 3 URL")]
        public string? Picture3 { get; set; }

        [Display(Name = "Picture 4 URL")]
        public string? Picture4 { get; set; }

        [Display(Name = "Picture 5 URL")]
        public string? Picture5 { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Range(0, 5000)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Discount %")]
        public decimal Discount { get; set; }
    }
}