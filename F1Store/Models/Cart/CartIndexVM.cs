using System.Collections.Generic;

namespace F1Store.Models.Cart
{
    public class CartIndexVM
    {
        public List<CartItemVM> Items { get; set; } = new List<CartItemVM>();
    }

    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}