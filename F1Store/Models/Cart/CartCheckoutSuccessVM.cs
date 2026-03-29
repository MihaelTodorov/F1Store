namespace F1Store.Models.Cart
{
    public class CartCheckoutSuccessVM
    {
        public Guid OrderGroupId { get; set; }

        public string OrderDate { get; set; } = null!;

        public List<CartCheckoutSuccessItemVM> Items { get; set; } = new();

        public int TotalProducts => Items.Sum(x => x.Quantity);

        public decimal TotalAmount => Items.Sum(x => x.Subtotal);
    }

    public class CartCheckoutSuccessItemVM
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string? Picture { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal FinalUnitPrice => UnitPrice * (1 - Discount / 100m);

        public decimal Subtotal => FinalUnitPrice * Quantity;
    }
}
