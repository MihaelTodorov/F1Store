using F1Store.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace F1Store.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService cartService;

        public CartSummaryViewComponent(ICartService cartService)
        {
            this.cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var claimsUser = HttpContext?.User;

            if (claimsUser?.Identity?.IsAuthenticated != true)
            {
                return View(0);
            }

            var userId = claimsUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return View(0);
            }

            var count = cartService.GetCart(userId).Sum(x => x.Quantity);

            return View(count);
        }
    }
}
