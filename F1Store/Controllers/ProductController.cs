using F1Store.Core.Contacts;
using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Models.Category;
using F1Store.Models.Product;
using F1Store.Models.Team;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace F1Store.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ITeamService _teamService;
        private readonly IFavoritesService _favoritesService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ITeamService teamService,
            IFavoritesService favoritesService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._teamService = teamService;
            this._favoritesService = favoritesService;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Index(string searchTerm, List<string> searchTeams, List<string> searchCategories, string team)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allProducts = _productService.GetProducts().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                allProducts = allProducts.Where(p => p.ProductName.ToLower().Contains(searchTerm));
            }

            var teamFilters = searchTeams ?? new List<string>();
            if (!string.IsNullOrEmpty(team) && !teamFilters.Contains(team))
            {
                teamFilters.Add(team);
            }

            if (teamFilters.Any())
            {
                allProducts = allProducts.Where(p => p.Team != null && teamFilters.Contains(p.Team.TeamName));
            }

            if (searchCategories != null && searchCategories.Any())
            {
                allProducts = allProducts.Where(p => p.Category != null && searchCategories.Contains(p.Category.CategoryName));
            }

            var productsList = allProducts.ToList();
            var viewModels = new List<ProductIndexVM>();

            foreach (var product in productsList)
            {
                viewModels.Add(new ProductIndexVM
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    TeamName = product.Team?.TeamName ?? "F1 Team", 
                    Picture = product.Picture,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    Discount = product.Discount,
                    IsFavorite = userId != null ? _favoritesService.IsFavorite(userId, product.Id) : false
                });
            }

            return View(viewModels);
        }

        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            var item = _productService.GetProductById(id);
            if (item == null) return NotFound();

            var model = new ProductDetailsVM()
            {
                Id = item.Id,
                ProductName = item.ProductName,
                TeamName = item.Team?.TeamName ?? "Unknown",
                CategoryName = item.Category?.CategoryName ?? "Unknown",
                Picture = item.Picture,
                Picture2 = item.Picture2,
                Picture3 = item.Picture3,
                Picture4 = item.Picture4,
                Picture5 = item.Picture5,
                Description = item.Description,
                Price = item.Price,
                Quantity = item.Quantity,
                Discount = item.Discount
            };

            return View(model);
        }


        public ActionResult Create()
        {
            var product = new ProductCreateVM();
            LoadDropdowns(product);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] ProductCreateVM product)
        {
            if (ModelState.IsValid)
            {
                var created = _productService.Create(
                    product.ProductName, product.TeamId, product.CategoryId,
                    product.Picture, product.Picture2, product.Picture3, product.Picture4, product.Picture5,
                    product.Description, product.Quantity, product.Price, product.Discount);

                if (created) return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(product);
            return View(product);
        }

        public ActionResult Edit(int id)
        {
            Product product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            ProductEditVM updatedProduct = new ProductEditVM()
            {
                Id = product.Id,
                ProductName = product.ProductName,
                TeamId = product.TeamId,
                CategoryId = product.CategoryId,
                Picture = product.Picture,
                Picture2 = product.Picture2,
                Picture3 = product.Picture3,
                Picture4 = product.Picture4,
                Picture5 = product.Picture5,
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price,
                Discount = product.Discount
            };

            LoadDropdowns(updatedProduct);
            return View(updatedProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ProductEditVM product)
        {
            if (ModelState.IsValid)
            {
                var updated = _productService.Update(
                    id, product.ProductName, product.TeamId, product.CategoryId,
                    product.Picture, product.Picture2, product.Picture3, product.Picture4, product.Picture5,
                    product.Description, product.Quantity, product.Price, product.Discount);

                if (updated) return RedirectToAction("Index");
            }

            LoadDropdowns(product);
            return View(product);
        }

        public ActionResult Delete(int id)
        {
            Product item = _productService.GetProductById(id);
            if (item == null) return NotFound();

            return View(new ProductDeleteVM()
            {
                Id = item.Id,
                ProductName = item.ProductName,
                TeamName = item.Team.TeamName,
                CategoryName = item.Category.CategoryName,
                Picture = item.Picture,
                Price = item.Price
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            if (_productService.RemoveById(id)) return RedirectToAction("Success");
            return View();
        }

        public IActionResult Success() => View("ProductSuccess");

        
        private void LoadDropdowns(dynamic vm)
        {
            vm.Teams = _teamService.GetTeams().Select(x => new TeamPairVM { Id = x.Id, Name = x.TeamName }).ToList();
            vm.Categories = _categoryService.GetCategories().Select(x => new CategoryPairVM { Id = x.Id, Name = x.CategoryName }).ToList();
        }
    }
}