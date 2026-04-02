using F1Store.Core.Contracts;
using F1Store.Core.Services;
using F1Store.Infrastructure.Data;
using F1Store.Models.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace F1Store.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;

        public CartController(
            ICartService cartService,
            IOrderService orderService,
            ApplicationDbContext context)
        {
            _cartService = cartService;
            _orderService = orderService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

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
        public IActionResult Add(int productId, int quantity = 1, string? returnUrl = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            if (quantity < 1) quantity = 1;

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToSafe(returnUrl);
            }

            var alreadyInCart = _context.CartItems
                .Where(ci => ci.UserId == userId && ci.ProductId == productId)
                .Select(ci => ci.Quantity)
                .FirstOrDefault();

            if (product.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "This product is out of stock.";
                return RedirectToSafe(returnUrl);
            }

            if (alreadyInCart + quantity > product.Quantity)
            {
                TempData["ErrorMessage"] = $"You already have {alreadyInCart} in your cart. Only {product.Quantity} total are available.";
                return RedirectToSafe(returnUrl);
            }

            var ok = _cartService.Add(productId, userId, quantity);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Could not add item (stock changed).";
                return RedirectToSafe(returnUrl);
            }

            TempData["SuccessMessage"] = "Added to cart.";
            return RedirectToSafe(returnUrl);
        }

        [HttpPost]
        [Authorize]
        public IActionResult DirectCheckout(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Добавяме продукта
            _cartService.Add(productId, userId, 1);

            // 2. Създаваме поръчката (връща bool)
            var ok = _orderService.CreateFromCart(userId);
            if (!ok) return RedirectToAction(nameof(Index));

            // 3. ВЗИМАМЕ ИСТИНСКОТО ID (както направихме в обикновения Checkout)
            var realGroupId = _orderService.GetLatestOrderGroupIdByUser(userId);

            // 4. ПРАЩАМЕ КЪМ ПЛАЩАНЕ с истинското ID
            return RedirectToAction("Payment", new { orderGroupId = realGroupId });
        }

        // 2. Страница за въвеждане на данни за карта
        [HttpGet]
        public IActionResult Payment(Guid orderGroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _orderService.GetOrdersByGroupId(orderGroupId, userId);

            if (orders == null || !orders.Any())
                return RedirectToAction(nameof(Index));

            var vm = new CartCheckoutSuccessVM
            {
                OrderGroupId = orderGroupId.ToString().ToUpper(), // Конвертиране към string за VM
                TotalAmount = orders.Sum(o => o.Quantity * (o.Price * (1 - o.Discount / 100m)))
            };

            return View(vm);
        }

        // 3. Обработка на плащането и пренасочване към финала
        [HttpPost]
        public IActionResult ProcessPayment(Guid orderGroupId)
        {
            // Тук плащането се счита за успешно и отиваме към Success екшъна
            return RedirectToAction("Success", new { orderGroupId = orderGroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var cart = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            foreach (var ci in cart)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == ci.ProductId);
                if (product == null || product.Quantity <= 0) _context.CartItems.Remove(ci);
                else if (ci.Quantity > product.Quantity) ci.Quantity = product.Quantity;
            }
            _context.SaveChanges();

            var ok = _orderService.CreateFromCart(userId);
            if (!ok) return RedirectToAction(nameof(Index));

            var groupId = _orderService.GetLatestOrderGroupIdByUser(userId);

            // Пренасочване към Payment вместо директно към Success
            return RedirectToAction("Payment", new { orderGroupId = groupId });
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

            // ВАЖНО: Изрично зареждаме преименуваното View
            return View("CartSuccess", vm);
        }

        // --- Ajax методи и Helper-и ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAjax(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Unauthorized." });

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound(new { message = "Product not found." });

            var alreadyInCart = _context.CartItems
                .Where(ci => ci.UserId == userId && ci.ProductId == productId)
                .Select(ci => ci.Quantity)
                .FirstOrDefault();

            if (product.Quantity <= 0 || (alreadyInCart + quantity > product.Quantity))
                return BadRequest(new { message = "Insufficient stock." });

            var ok = _cartService.Add(productId, userId, quantity);
            return ok ? Ok(new { message = "Added to cart." }) : BadRequest();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantityAjax(int cartItemId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            if (quantity < 1) quantity = 1;

            var item = _context.CartItems.FirstOrDefault(x => x.Id == cartItemId && x.UserId == userId);
            if (item == null) return BadRequest();

            var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null) return BadRequest();

            if (quantity > product.Quantity)
            {
                if (product.Quantity <= 0)
                {
                    _cartService.Remove(cartItemId, userId);
                    return Ok(new { removed = true, total = _cartService.GetTotal(userId) });
                }
                quantity = product.Quantity;
            }

            _cartService.UpdateQuantity(cartItemId, userId, quantity);
            var updated = _cartService.GetCart(userId).FirstOrDefault(x => x.Id == cartItemId);

            decimal finalPrice = updated.Price * (1 - updated.Discount / 100m);
            return Ok(new
            {
                quantity = updated.Quantity,
                subtotal = updated.Quantity * finalPrice,
                total = _cartService.GetTotal(userId),
                available = product.Quantity
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _cartService.Remove(cartItemId, userId);
            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectToSafe(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}