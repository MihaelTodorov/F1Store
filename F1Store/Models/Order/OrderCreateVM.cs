using System.ComponentModel.DataAnnotations;

namespace F1Store.Models.Order
{
    public class OrderCreateVM
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int QuantityInStock { get; set; }
        public string Picture { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}