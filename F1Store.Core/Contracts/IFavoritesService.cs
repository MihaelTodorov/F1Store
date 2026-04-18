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
        IEnumerable<Favorite> GetUserFavorites(string userId);
        void AddToFavorites(string userId, int productId);
        void RemoveFromFavorites(string userId, int productId);
        bool IsFavorite(string userId, int productId);
    }
}
