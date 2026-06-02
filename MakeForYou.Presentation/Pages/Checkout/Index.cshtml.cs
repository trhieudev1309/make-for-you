using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Checkout
{
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public IndexModel(ICartService cartService, IOrderService orderService, IPaymentService paymentService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public int TotalAmount { get; set; }

        [BindProperty]
        public CheckoutRequest OrderRequest { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");

            long userId = long.Parse(userIdStr);
            CartItems = await _cartService.GetCartAsync(userId);
            if (!CartItems.Any()) return RedirectToPage("/Cart/Index");

            TotalAmount = CartItems.Sum(x => x.TotalPrice);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");
            long userId = long.Parse(userIdStr);

            var cartItems = await _cartService.GetCartAsync(userId);
            if (!cartItems.Any()) return RedirectToPage("/Cart/Index");

            // Generate a unique payment code (10-digit, fits PayOS orderCode constraints)
            var paymentCode = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L);

            var orders = await _orderService.CreateOrderFromCartAsync(
                userId,
                OrderRequest.FullName,
                OrderRequest.PhoneNumber,
                OrderRequest.ShippingAddress,
                paymentCode);

            if (orders == null || !orders.Any()) return Page();

            var totalAmount = orders.Sum(o => o.AgreedPrice ?? 0);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var checkoutUrl = await _paymentService.CreatePaymentLinkAsync(paymentCode, totalAmount, baseUrl, cartItems);

            return Redirect(checkoutUrl);
        }
    }
}
