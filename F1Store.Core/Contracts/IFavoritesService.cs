using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Contracts
{
    public interface IFavoritesService
    {
        Task<IEnumerable<Favorite>> GetUserFavoritesAsync(string userId);
        Task AddToFavoritesAsync(string userId, int productId);
        Task RemoveFromFavoritesAsync(string userId, int productId);
        Task<bool> IsFavoriteAsync(string userId, int productId);
    }
}
