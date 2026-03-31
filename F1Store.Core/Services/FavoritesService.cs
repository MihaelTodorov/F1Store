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
    namespace F1Store.Services
    {
        public class FavoritesService : IFavoritesService
        {
            private readonly ApplicationDbContext _context;

            public FavoritesService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<Favorite>> GetUserFavoritesAsync(string userId)
            {
                return await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.Product)
                    .ToListAsync();
            }

            public async Task AddToFavoritesAsync(string userId, int productId)
            {
                if (!await IsFavoriteAsync(userId, productId))
                {
                    var favorite = new Favorite
                    {
                        UserId = userId,
                        ProductId = productId
                    };
                    _context.Favorites.Add(favorite);
                    await _context.SaveChangesAsync();
                }
            }

            public async Task RemoveFromFavoritesAsync(string userId, int productId)
            {
                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (favorite != null)
                {
                    _context.Favorites.Remove(favorite);
                    await _context.SaveChangesAsync();
                }
            }

            public async Task<bool> IsFavoriteAsync(string userId, int productId)
            {
                return await _context.Favorites
                    .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
            }
        }
    }
}
