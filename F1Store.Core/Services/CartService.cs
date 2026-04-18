using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data;
using F1Store.Infrastructure.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace F1Store.Core.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CartItem> GetCart(string userId)
            => _context.CartItems
            .Include(x => x.Product)
            .Where(x => x.UserId == userId)
            .ToList();

        public int GetCartCount(string userId)
            => _context.CartItems.Where(ci => ci.UserId == userId).Sum(ci => ci.Quantity);

        public decimal GetTotal(string userId)
        {
            var items = GetCart(userId);
            return items.Sum(i => i.Quantity * i.Price - i.Quantity * i.Price * i.Discount / 100m);
        }

        public bool Add(int productId, string userId, int quantity = 1)
        {
            var product = _context.Products.SingleOrDefault(p => p.Id == productId);
            if (product == null || product.Quantity <= 0) return false;

            if (quantity < 1) quantity = 1;

            var existing = _context.CartItems.SingleOrDefault(x => x.UserId == userId && x.ProductId == productId);
            var requestedTotal = quantity + (existing?.Quantity ?? 0);

            if (requestedTotal > product.Quantity) return false;

            if (existing == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price,
                    Discount = product.Discount
                });
            }
            else
            {
                existing.Quantity += quantity;
                existing.Price = product.Price;
                existing.Discount = product.Discount;
                // ТУК: Премахнат _context.CartItems.Update(existing); - по препоръка на учителката
            }

            return _context.SaveChanges() != 0;
        }

        // ... останалите методи (UpdateQuantity, Remove, Clear) също без .Update() ...
        public bool Remove(int cartItemId, string userId)
        {
            var item = _context.CartItems.SingleOrDefault(x => x.Id == cartItemId && x.UserId == userId);
            if (item == null) return false;

            _context.CartItems.Remove(item);
            return _context.SaveChanges() != 0;
        }

        public bool Clear(string userId)
        {
            var items = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            _context.CartItems.RemoveRange(items);
            return _context.SaveChanges() != 0;
        }

        public bool UpdateQuantity(int cartItemId, string userId, int quantity)
        {
            var item = _context.CartItems.Include(ci => ci.Product).SingleOrDefault(x => x.Id == cartItemId && x.UserId == userId);
            if (item == null || quantity > item.Product.Quantity) return false;

            if (quantity < 1) return Remove(cartItemId, userId);

            item.Quantity = quantity;
            return _context.SaveChanges() != 0;
        }
    }
}