using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Infrastructure.Data;
using F1Store.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace F1Store.Core.Services
{
    public class FavoritesService : IFavoritesService
    {
        private readonly ApplicationDbContext _context;

        public FavoritesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Favorite> GetUserFavorites(string userId)
        {
            return _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ToList(); 
        }

        public void AddToFavorites(string userId, int productId)
        {
            if (!IsFavorite(userId, productId))
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId
                };
                _context.Favorites.Add(favorite);
                _context.SaveChanges(); 
            }
        }

        public void RemoveFromFavorites(string userId, int productId)
        {
            var favorite = _context.Favorites
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                _context.SaveChanges();
            }
        }

        public bool IsFavorite(string userId, int productId)
        {
            return _context.Favorites
                .Any(f => f.UserId == userId && f.ProductId == productId);
        }
    }
}
