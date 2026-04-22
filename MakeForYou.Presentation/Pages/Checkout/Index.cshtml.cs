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

        public IndexModel(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
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

            // Task của Tiến: Tạo đơn hàng
            var orders = await _orderService.CreateOrderFromCartAsync(
                userId,
                OrderRequest.FullName,
                OrderRequest.PhoneNumber,
                OrderRequest.ShippingAddress);

            if (orders == null || !orders.Any()) return Page();

            // Chuyển thẳng về Success sau khi lưu DB thành công
            return RedirectToPage("/Checkout/Success", new { orderId = orders.First().OrderId });
        }
    }
}