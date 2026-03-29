namespace F1Store.Models.Cart
{
    public class CartItemVM
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;
        public string? Picture { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }

        public int AvailableQuantity { get; set; }

        public bool IsOutOfStock => AvailableQuantity <= 0;
        public bool IsInsufficient => !IsOutOfStock && Quantity > AvailableQuantity;

        public decimal FinalUnitPrice => UnitPrice * (1 - Discount / 100m);
        public decimal Subtotal => FinalUnitPrice * Quantity;
    }

}
