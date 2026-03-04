using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Contracts
{
    public interface ICartService
    {
        Task<Cart> GetCartAsync(string userId);
        Task AddItemAsync(string userId, int productId, string name, decimal price, int quantity = 1);
        Task RemoveItemAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }
}
