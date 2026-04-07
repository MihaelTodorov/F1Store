using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

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

            if (currentUserId == null || product == null || product.Quantity < bindingModel.Quantity || product.Quantity == 0)
            {
                return RedirectToAction("Denied", "Order");
            }

            if (ModelState.IsValid)
            {
                _orderService.Create(bindingModel.ProductId, currentUserId, bindingModel.Quantity);

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
            var rawOrders = _orderService.GetOrdersByUser(currentUserId);

            var orders = rawOrders.Select(x => new OrderIndexVM
            {
                Id = x.Id,
                OrderDate = x.OrderDate.ToString("dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture),
                UserId = x.UserId,
                User = x.User?.UserName ?? "Unknown User",
                ProductId = x.ProductId,
                Product = x.Product?.ProductName ?? "Product Deleted",
                Picture = x.Product?.Picture ?? "/images/no-image.png",
                Quantity = x.Quantity,
                Price = x.Price,
                Discount = x.Discount,
                TotalPrice = (x.Price * x.Quantity) * (1 - (decimal)x.Discount / 100)
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

                _productService.Update(
                    product.Id,
                    product.ProductName,
                    product.TeamId,
                    product.CategoryId,
                    product.Picture,
                    product.Picture2,
                    product.Picture3,
                    product.Picture4,
                    product.Picture5,
                    product.Description,
                    product.Quantity,
                    product.Price,
                    product.Discount);
            }

            _orderService.Delete(order.Id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var order = _orderService.GetOrderDetails(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}