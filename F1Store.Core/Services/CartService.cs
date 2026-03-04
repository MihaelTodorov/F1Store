using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Services
{
    public class CartService : ICartService
    {
        // In-memory storage for demonstration. Replace with DB or session as needed.
        private static readonly ConcurrentDictionary<string, Cart> carts = new();

        public Task<Cart> GetCartAsync(string userId)
        {
            carts.TryGetValue(userId, out var cart);
            return Task.FromResult(cart ?? new Cart { UserId = userId });
        }

        public Task AddItemAsync(string userId, int productId, string name, decimal price, int quantity = 1)
        {
            var cart = carts.GetOrAdd(userId, new Cart { UserId = userId });
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                cart.Items.Add(new CartItem { ProductId = productId, Name = name, Price = price, Quantity = quantity });
            }
            else
            {
                item.Quantity += quantity;
            }
            return Task.CompletedTask;
        }

        public Task RemoveItemAsync(string userId, int productId)
        {
            if (carts.TryGetValue(userId, out var cart))
            {
                cart.Items.RemoveAll(i => i.ProductId == productId);
            }
            return Task.CompletedTask;
        }

        public Task ClearCartAsync(string userId)
        {
            if (carts.TryGetValue(userId, out var cart))
            {
                cart.Items.Clear();
            }
            return Task.CompletedTask;
        }
    }
}
