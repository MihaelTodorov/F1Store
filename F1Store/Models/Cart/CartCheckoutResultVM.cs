namespace F1Store.Models.Cart
{
    public class CartCheckoutResult
    {
        public bool Success { get; set; }
        public Guid? OrderGroupId { get; set; }
        public List<CartStockIssue> Issues { get; set; } = new();
    }

    public class CartStockIssue
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Requested { get; set; }
        public int Available { get; set; }
        public string Action { get; set; } = string.Empty; // "Removed" / "Adjusted"
    }
}
