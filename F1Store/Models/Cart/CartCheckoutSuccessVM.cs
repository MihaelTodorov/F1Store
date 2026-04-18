namespace F1Store.Models.Cart
{
    public class CartCheckoutSuccessVM
    {
        public string OrderGroupId { get; set; }
        public string OrderDate { get; set; }
        public decimal TotalAmount { get; set; } 
        public int TotalProducts => Items.Sum(i => i.Quantity);
        public List<CartCheckoutSuccessItemVM> Items { get; set; } = new();
    }

    public class CartCheckoutSuccessItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? Picture { get; set; } 
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalUnitPrice => UnitPrice * (1 - Discount / 100m);
        public decimal Subtotal => Quantity * FinalUnitPrice;
    }
}