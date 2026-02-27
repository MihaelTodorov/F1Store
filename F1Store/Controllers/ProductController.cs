using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Models.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using F1Store.Models.Team;
using F1Store.Models.Product;
using F1Store.Core.Contacts;

namespace F1Store.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ITeamService _teamService;

        public ProductController(IProductService productService, ICategoryService categoryService, ITeamService teamService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._teamService = teamService;
        }

        [AllowAnonymous]
        public ActionResult Index(string searchStringCategoryName, string searchStringTeamName)
        {
            List<ProductIndexVM> products = _productService.GetProducts(searchStringCategoryName, searchStringTeamName)
                .Select(product => new ProductIndexVM
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    TeamId = product.TeamId,
                    TeamName = product.Team.TeamName,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.CategoryName,
                    Picture = product.Picture,
                    Discription = product.Discription,
                    Quantity = product.Quantity,
                    Price = product.Price,
                    Discount = product.Discount
                }).ToList();

            return this.View(products);
        }


        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            Product item = _productService.GetProductById(id);
            if (item == null)
            {
                return NotFound();
            }

            ProductDetailsVM product = new ProductDetailsVM()
            {
                Id = item.Id,
                ProductName = item.ProductName,
                TeamId = item.TeamId,
                TeamName = item.Team.TeamName,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.CategoryName,
                Picture = item.Picture,
                Discription = item.Discription,
                Quantity = item.Quantity,
                Price = item.Price,
                Discount = item.Discount
            };

            return View(product);
        }

        public ActionResult Create()
        {
            var product = new ProductCreateVM();

            product.Teams = _teamService.GetTeams()
                .Select(x => new TeamPairVM()
                {
                    Id = x.Id,
                    Name = x.TeamName
                }).ToList();

            product.Categories = _categoryService.GetCategories()
                .Select(x => new CategoryPairVM()
                {
                    Id = x.Id,
                    Name = x.CategoryName
                }).ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] ProductCreateVM product)
        {
            if (ModelState.IsValid)
            {
                var createdId = _productService.Create(product.ProductName, product.TeamId,
                    product.CategoryId, product.Picture, product.Discription,
                    product.Quantity, product.Price, product.Discount);
                if (createdId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View();
        }

        public ActionResult Edit(int id)
        {
            Product product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            ProductEditVM updatedProduct = new ProductEditVM()
            {
                Id = product.Id,
                ProductName = product.ProductName,
                TeamId = product.TeamId,
                CategoryId = product.CategoryId,
                Picture = product.Picture,
                Discription = product.Discription,
                Quantity = product.Quantity,
                Price = product.Price,
                Discount = product.Discount
            };
            updatedProduct.Teams = _teamService.GetTeams()
                    .Select(b => new TeamPairVM { Id = b.Id, Name = b.TeamName }).ToList();
            updatedProduct.Categories = _categoryService.GetCategories()
                    .Select(c => new CategoryPairVM { Id = c.Id, Name = c.CategoryName }).ToList();

            return View(updatedProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ProductEditVM product)
        {
            if (ModelState.IsValid)
            {
                var updated = _productService.Update(id, product.ProductName, product.TeamId,
                    product.CategoryId, product.Picture, product.Discription, product.Quantity, product.Price, product.Discount);
                if (updated)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(product);
        }

        public ActionResult Delete(int id)
        {
            Product item = _productService.GetProductById(id);
            if (item == null)
            {
                return NotFound();
            }

            ProductDeleteVM product = new ProductDeleteVM()
            {
                Id = item.Id,
                ProductName = item.ProductName,
                TeamId = item.TeamId,
                TeamName = item.Team.TeamName,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.CategoryName,
                Picture = item.Picture,
                Discription = item.Discription,
                Quantity = item.Quantity,
                Price = item.Price,
                Discount = item.Discount
            };
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            var deleted = _productService.RemoveById(id);

            if (deleted)
            {
                return this.RedirectToAction("Success");
            }
            else
            {
                return View();
            }
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
