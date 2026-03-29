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

                    // NEW
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

            // current quantity already in cart
            var alreadyInCart = _context.CartItems
                .Where(ci => ci.UserId == userId && ci.ProductId == productId)
                .Select(ci => ci.Quantity)
                .FirstOrDefault();

            if (product.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "This product is out of stock.";
                return RedirectToSafe(returnUrl);
            }

            // user tries to exceed stock
            if (alreadyInCart + quantity > product.Quantity)
            {
                TempData["ErrorMessage"] =
                    $"You already have {alreadyInCart} in your cart. Only {product.Quantity} total are available.";
                return RedirectToSafe(returnUrl);
            }

            var ok = _cartService.Add(productId, userId, quantity);

            if (!ok)
            {
                // Safety fallback (shouldn’t happen due to checks above)
                TempData["ErrorMessage"] = "Could not add item (stock changed). Please try again.";
                return RedirectToSafe(returnUrl);
            }

            TempData["SuccessMessage"] = "Added to cart.";
            return RedirectToSafe(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAjax(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Unauthorized." });

            if (quantity < 1) quantity = 1;

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            var alreadyInCart = _context.CartItems
                .Where(ci => ci.UserId == userId && ci.ProductId == productId)
                .Select(ci => ci.Quantity)
                .FirstOrDefault();

            if (product.Quantity <= 0)
                return BadRequest(new { message = "This product is out of stock." });

            if (alreadyInCart + quantity > product.Quantity)
                return BadRequest(new
                {
                    message = $"You already have {alreadyInCart} in your cart. Only {product.Quantity} total are available."
                });

            var ok = _cartService.Add(productId, userId, quantity);
            if (!ok)
                return BadRequest(new { message = "Could not add item (stock changed). Please try again." });

            return Ok(new { message = "Added to cart." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantityAjax(int cartItemId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Unauthorized." });

            if (quantity < 1) quantity = 1;

            var item = _context.CartItems.FirstOrDefault(x => x.Id == cartItemId && x.UserId == userId);
            if (item == null)
                return BadRequest(new { message = "Cart item not found." });

            var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                return BadRequest(new { message = "Product not found." });

            // ✅ IMPORTANT: allow decreasing even if stock is 0 / changed
            // If requested > stock -> clamp to stock (and if stock == 0 remove)
            if (quantity > product.Quantity)
            {
                if (product.Quantity <= 0)
                {
                    _cartService.Remove(cartItemId, userId);
                    return Ok(new
                    {
                        removed = true,
                        message = "Item removed: product is out of stock.",
                        total = _cartService.GetTotal(userId)
                    });
                }

                quantity = product.Quantity;
            }

            var ok = _cartService.UpdateQuantity(cartItemId, userId, quantity);
            if (!ok)
                return BadRequest(new { message = "Could not update quantity." });

            // reload totals
            var updated = _cartService.GetCart(userId).FirstOrDefault(x => x.Id == cartItemId);
            if (updated == null)
                return BadRequest(new { message = "Cart item not found." });

            decimal finalUnitPrice = updated.Price * (1 - updated.Discount / 100m);
            decimal subtotal = updated.Quantity * finalUnitPrice;
            decimal total = _cartService.GetTotal(userId);

            return Ok(new
            {
                quantity = updated.Quantity,
                finalUnitPrice,
                subtotal,
                total,
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
        public IActionResult Clear()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _cartService.Clear(userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            // Auto-adjust before checkout (no full-page “not allowed”)
            var cart = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            var changes = new List<string>();

            foreach (var ci in cart)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == ci.ProductId);
                if (product == null) continue;

                if (product.Quantity <= 0)
                {
                    _context.CartItems.Remove(ci);
                    changes.Add($"{ci.ProductId}: removed (out of stock)");
                    continue;
                }

                if (ci.Quantity > product.Quantity)
                {
                    ci.Quantity = product.Quantity;
                    _context.CartItems.Update(ci);
                    changes.Add($"{product.ProductName}: adjusted to {product.Quantity}");
                }
            }

            _context.SaveChanges();

            // now try create
            var ok = _orderService.CreateFromCart(userId);

            if (!ok)
            {
                TempData["ErrorMessage"] = changes.Count > 0
                    ? "Stock updated: " + string.Join(" | ", changes)
                    : "Could not checkout. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            var groupId = _orderService.GetLatestOrderGroupIdByUser(userId);
            if (groupId == null)
                return RedirectToAction(nameof(Index));

            return RedirectToAction(nameof(Success), new { orderGroupId = groupId.Value });
        }

        [HttpGet]
        public IActionResult Success(Guid orderGroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var orders = _orderService.GetOrdersByGroupId(orderGroupId, userId);
            if (orders == null || orders.Count == 0)
                return RedirectToAction(nameof(Index));

            var first = orders.First();

            var vm = new CartCheckoutSuccessVM
            {
                OrderGroupId = orderGroupId,
                OrderDate = first.OrderDate.ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture),
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

            // referer fallback
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer) &&
                Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            {
                var local = uri.PathAndQuery;
                if (Url.IsLocalUrl(local))
                    return Redirect(local);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
