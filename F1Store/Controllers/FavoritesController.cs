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

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Вземаме любимите от базата чрез сервиза
            var favorites = await _favoritesService.GetUserFavoritesAsync(userId!);

            // Създаваме новия VM
            var viewModel = new FavoriteVM
            {
                Items = favorites.Select(f => new FavoriteItemVM
                {
                    ProductId = f.ProductId,
                    ProductName = f.Product.ProductName, // Корекция: ProductName вместо Name
                    Picture = f.Product.Picture,         // Корекция: Picture вместо ImageUrl
                    Price = f.Product.Price
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await _favoritesService.AddToFavoritesAsync(userId, productId);
            }
            return Redirect(Request.Headers["Referer"].ToString() ?? "/Product/Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await _favoritesService.RemoveFromFavoritesAsync(userId, productId);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}