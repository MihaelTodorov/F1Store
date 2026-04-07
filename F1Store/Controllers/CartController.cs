using F1Store.Core.Contracts;
using F1Store.Core.Services;
using F1Store.Infrastructure.Data;
using F1Store.Models.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace F1Store.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;

        public CartController(
            ICartService cartService,
            IOrderService orderService,
            IProductService productService,
            ApplicationDbContext context)
        {
            _cartService = cartService;
            _orderService = orderService;
            _productService = productService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var items = _cartService.GetCart(userId)
                .Select(ci => new CartItemVM
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.ProductName,
                    Picture = ci.Product.Picture,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Price,
                    Discount = ci.Discount,
                    AvailableQuantity = ci.Product.Quantity
                })
                .ToList();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAjax(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Моля, влезте в профила си." });

            if (quantity < 1) quantity = 1;

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound(new { message = "Продуктът не е намерен." });

            var alreadyInCart = _context.CartItems
                .Where(ci => ci.UserId == userId && ci.ProductId == productId)
                .Sum(ci => ci.Quantity);

            if (product.Quantity <= 0)
                return BadRequest(new { message = "Продуктът е изчерпан." });

            if (alreadyInCart + quantity > product.Quantity)
                return BadRequest(new { message = $"Налични са само {product.Quantity} бройки." });

            var ok = _cartService.Add(productId, userId, quantity);

            if (ok)
            {
                // ВАЖНО: Тук сумираме всички Quantity, вместо просто да броим редовете
                var totalItemsCount = _cartService.GetCart(userId).Sum(ci => ci.Quantity);

                return Ok(new
                {
                    success = true,
                    message = "BOX BOX! Добавено в количката.",
                    count = totalItemsCount // Вече ще връща реалната бройка (напр. 9)
                });
            }

            return BadRequest(new { message = "Грешка при добавяне." });
        }


        // Стандартен Add (за всеки случай, ако някъде не ползваш Ajax)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var ok = _cartService.Add(productId, userId, quantity);
            if (!ok) TempData["ErrorMessage"] = "Грешка при добавяне.";
            else TempData["SuccessMessage"] = "Добавено!";

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var ok = _orderService.CreateFromCart(userId);
            if (!ok) return RedirectToAction(nameof(Index));

            var groupId = _orderService.GetLatestOrderGroupIdByUser(userId);
            if (groupId == null) return RedirectToAction(nameof(Index));

            return RedirectToAction("Payment", new { orderGroupId = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DirectCheckout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var ok = _orderService.CreateFromCart(userId);
            if (!ok) return RedirectToAction(nameof(Index));

            var groupId = _orderService.GetLatestOrderGroupIdByUser(userId);
            if (groupId == null) return RedirectToAction(nameof(Index));

            return RedirectToAction("Payment", new { orderGroupId = groupId });
        }

        [HttpGet]
        public IActionResult Payment(Guid orderGroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _orderService.GetOrdersByGroupId(orderGroupId, userId);

            if (orders == null || !orders.Any())
                return RedirectToAction(nameof(Index));

            var vm = new CartCheckoutSuccessVM
            {
                OrderGroupId = orderGroupId.ToString().ToUpper(),
                TotalAmount = orders.Sum(o => o.Quantity * (o.Price * (1 - o.Discount / 100m)))
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string orderGroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = _context.Orders
                .Include(o => o.Product)
                .Where(o => o.OrderGroupId == Guid.Parse(orderGroupId) && o.UserId == userId)
                .ToList();

            foreach (var order in orders)
            {
                order.Product.Quantity -= order.Quantity;
            }

            var cartItems = _context.CartItems.Where(ci => ci.UserId == userId);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { orderGroupId = orderGroupId });
        }

        [HttpGet]
        public IActionResult Success(Guid orderGroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _orderService.GetOrdersByGroupId(orderGroupId, userId);

            if (orders == null || !orders.Any())
                return RedirectToAction(nameof(Index));

            var first = orders.First();
            var vm = new CartCheckoutSuccessVM
            {
                OrderGroupId = orderGroupId.ToString().ToUpper(),
                OrderDate = first.OrderDate.ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture),
                TotalAmount = orders.Sum(o => o.Quantity * (o.Price * (1 - o.Discount / 100m))),
                Items = orders.Select(o => new CartCheckoutSuccessItemVM
                {
                    ProductId = o.ProductId,
                    ProductName = o.Product.ProductName,
                    Picture = o.Product.Picture,
                    Quantity = o.Quantity,
                    UnitPrice = o.Price,
                    Discount = o.Discount
                }).ToList()
            };

            return View("CartSuccess", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _cartService.Remove(cartItemId, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearEntireCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _cartService.Clear(userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int delta)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var item = _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (item == null) return NotFound();

            int newQuantity = item.Quantity + delta;

            if (newQuantity < 1)
            {
                _context.CartItems.Remove(item);
            }
            else if (newQuantity > item.Product.Quantity)
            {
                TempData["ErrorMessage"] = "Няма достатъчна наличност!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                item.Quantity = newQuantity;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}