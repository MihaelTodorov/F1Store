namespace F1Store.Models.Order
{
    public class OrderDetailsVM
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string User { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Product { get; set; } = null!;
        public string Picture { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }

    }
}