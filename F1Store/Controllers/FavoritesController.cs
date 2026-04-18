using F1Store.Core.Contracts;
using F1Store.Models.Favorite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace F1Store.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly IFavoritesService _favoritesService;

        public FavoritesController(IFavoritesService favoritesService)
        {
            _favoritesService = favoritesService;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = _favoritesService.GetUserFavorites(userId!);

            var viewModel = new FavoriteVM
            {
                Items = favorites.Select(f => new FavoriteItemVM
                {
                    ProductId = f.ProductId,
                    ProductName = f.Product.ProductName,
                    Picture = f.Product.Picture,
                    Price = f.Product.Price
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            _favoritesService.AddToFavorites(userId, productId);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            _favoritesService.RemoveFromFavorites(userId, productId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}