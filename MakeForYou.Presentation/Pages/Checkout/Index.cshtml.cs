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

            // Tạo các đơn hàng (đã được group theo Seller bên trong Service)
            var orders = await _orderService.CreateOrderFromCartAsync(
                userId,
                OrderRequest.FullName,
                OrderRequest.PhoneNumber,
                OrderRequest.ShippingAddress);

            if (orders == null || !orders.Any()) return Page();

            // Lấy tất cả ID đơn hàng nối lại thành chuỗi "1,2,3"
            var allOrderIds = string.Join(",", orders.Select(o => o.OrderId));

            // Truyền chuỗi ID sang trang Success
            return RedirectToPage("/Checkout/Success", new { orderIds = allOrderIds });
        }
    }
}