using F1Store.Core.Contracts;
using F1Store.Models.Favorite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        // Страницата "Моите любими"
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _favoritesService.GetUserFavoritesAsync(userId!);

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
        public async Task<IActionResult> Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _favoritesService.AddToFavoritesAsync(userId, productId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _favoritesService.RemoveFromFavoritesAsync(userId, productId);

            // АКО е AJAX (от началната страница) -> върни JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            // АКО е обикновено натискане (от страницата Favorites) -> презареди страницата
            return RedirectToAction(nameof(Index));
        }
    }
}