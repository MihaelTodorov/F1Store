using System.ComponentModel.DataAnnotations;
using F1Store.Models.Category;
using F1Store.Models.Team;
using F1Store.Models.Category;

namespace F1Store.Models.Product
{
    public class ProductCreateVM
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

        [Display(Name = "Picture")]
        public string Picture { get; set; } = null!;

        [Display(Name = "Discriprion")]
        public string Discription { get; set; } = null!;

        [Range(0, 5000)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Discount")]
        public decimal Discount { get; set; }
    }
}