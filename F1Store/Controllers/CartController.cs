using F1Store.Core.Contracts;
using F1Store.Models.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace F1Store.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(
            ICartService cartService,
            IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
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

            var ok = _cartService.Add(productId, userId, quantity);

            if (ok)
            {
                return Ok(new
                {
                    success = true,
                    message = "BOX BOX! Добавено в количката.",
                    count = _cartService.GetCartCount(userId)
                });
            }

            return BadRequest(new { message = "Недостатъчна наличност или грешка." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessPayment(string orderGroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ВАЖНО: Тук трябва да извикаш метод от OrderService, 
            // който да намали наличностите и да изчисти количката.
            // НЕ ползвай _context директно тук!

            var ok = _orderService.FinalizePayment(Guid.Parse(orderGroupId), userId!);

            return RedirectToAction("Success", new { orderGroupId = orderGroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int cartItemId, int delta)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = _cartService.GetCart(userId!);
            var item = items.FirstOrDefault(i => i.Id == cartItemId);

            if (item != null)
            {
                _cartService.UpdateQuantity(cartItemId, userId!, item.Quantity + delta);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _cartService.Remove(cartItemId, userId!);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearEntireCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _cartService.Clear(userId!);
            return RedirectToAction(nameof(Index));
        }
    }
}