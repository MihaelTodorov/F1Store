using F1Store.Infrastructure.Data.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F1Store.Core.Contracts
{
    public interface ICartService
    {
        List<CartItem> GetCart(string userId);
        bool Add(int productId, string userId, int quantity = 1);
        bool UpdateQuantity(int cartItemId, string userId, int quantity);
        bool Remove(int cartItemId, string userId);
        bool Clear(string userId);
        decimal GetTotal(string userId);
    }
}
