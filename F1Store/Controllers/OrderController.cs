using System.Globalization;
using System.Security.Claims;
using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Models.Order;
using F1Store.Models.Cart; // Добави това, ако CartCheckoutSuccessVM е тук
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace F1Store.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public OrderController(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        // GET: Order/Create/5
        public ActionResult Create(int id)
        {
            Product product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            OrderCreateVM order = new OrderCreateVM()
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                QuantityInStock = product.Quantity,
                Price = product.Price,
                Discount = product.Discount,
                Picture = product.Picture,
            };
            return View(order);
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OrderCreateVM bindingModel)
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = this._productService.GetProductById(bindingModel.ProductId);

            // Проверка за наличност и валиден потребител
            if (currentUserId == null || product == null || product.Quantity < bindingModel.Quantity || product.Quantity == 0)
            {
                return RedirectToAction("Denied", "Order");
            }

            if (ModelState.IsValid)
            {
                // Записваме поръчката в базата
                _orderService.Create(bindingModel.ProductId, currentUserId, bindingModel.Quantity);

                // ВАЖНО: Пренасочваме към Success екшъна, за да се активира EmailJS
                return RedirectToAction(nameof(Success), new
                {
                    productId = bindingModel.ProductId,
                    qty = bindingModel.Quantity
                });
            }

            return View(bindingModel);
        }

        // GET: Order/Success
        public ActionResult Success(int productId, int qty)
        {
            // Тук можеш да подадеш данни към View-то, ако искаш да ги покажеш
            // или просто да заредиш View-то, което съдържа JS скрипта.
            return View();
        }

        public ActionResult Denied()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            List<OrderIndexVM> orders = _orderService.GetOrders()
                .Select(x => new OrderIndexVM
                {
                    Id = x.Id,
                    OrderDate = x.OrderDate.ToString("dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture),
                    UserId = x.UserId,
                    User = x.User.UserName,
                    ProductId = x.ProductId,
                    Product = x.Product.ProductName,
                    Picture = x.Product.Picture,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    Discount = x.Discount,
                    TotalPrice = x.TotalPrice,
                }).ToList();
            return View(orders);
        }

        public ActionResult MyOrders()
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<OrderIndexVM> orders = _orderService.GetOrdersByUser(currentUserId)
                .Select(x => new OrderIndexVM
                {
                    Id = x.Id,
                    OrderDate = x.OrderDate.ToString("dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture),
                    UserId = x.UserId,
                    User = x.User.UserName,
                    ProductId = x.ProductId,
                    Product = x.Product.ProductName,
                    Picture = x.Product.Picture,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    Discount = x.Discount,
                    TotalPrice = x.TotalPrice,
                }).ToList();
            return View(orders);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            OrderDeleteVM vm = new OrderDeleteVM()
            {
                Id = order.Id,
                OrderDate = order.OrderDate.ToString("dd-MM-yyyy HH:mm"),
                User = order.User.UserName,
                Product = order.Product.ProductName,
                Picture = order.Product.Picture,
                Quantity = order.Quantity,
                Price = order.Price,
                Discount = order.Discount,
                TotalPrice = order.TotalPrice
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(OrderDeleteVM bindingModel)
        {
            var order = _orderService.GetOrderById(bindingModel.Id);
            if (order == null) return NotFound();

            var product = _productService.GetProductById(order.ProductId);
            if (product != null)
            {
                product.Quantity += order.Quantity;
                _productService.Update(product.Id, product.ProductName, product.TeamId, product.CategoryId, product.Picture, product.Description, product.Quantity, product.Price, product.Discount);
            }

            _orderService.Delete(order.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}