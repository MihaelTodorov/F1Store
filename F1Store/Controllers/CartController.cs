using F1Store.Core.Contracts;
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

        // --- НОВ МЕТОД ЗА "BUY NOW" ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DirectCheckout(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null || product.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Продуктът не е наличен.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Добавяме го в количката (ако вече е там, услугата обикновено увеличава количеството)
            var ok = _cartService.Add(productId, userId, 1);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Грешка при добавяне в количката.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Директно изпълняваме Checkout логиката
            var orderOk = _orderService.CreateFromCart(userId);
            if (!orderOk)
            {
                TempData["ErrorMessage"] = "Грешка при създаване на поръчката.";
                return RedirectToAction(nameof(Index));
            }

            var groupId = _orderService.GetLatestOrderGroupIdByUser(userId);
            return RedirectToAction(nameof(Success), new { orderGroupId = groupId });
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var cart = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            // Проверка и корекция на наличности преди финализиране
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
            return RedirectToAction(nameof(Success), new { orderGroupId = groupId });
        }

        [HttpGet]
        public IActionResult Success(Guid orderGroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _orderService.GetOrdersByGroupId(orderGroupId, userId);
            if (orders == null || !orders.Any()) return RedirectToAction(nameof(Index));

            var first = orders.First();
            var vm = new CartCheckoutSuccessVM
            {
                OrderGroupId = orderGroupId.ToString,
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

            return View(vm);
        }

        private IActionResult RedirectToSafe(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}