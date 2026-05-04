using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;
        public IndexModel(ICartService cartService) { _cartService = cartService; }

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public int GrandTotal { get; set; }
        public int CartCount { get; set; }

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;

            CartItems = await _cartService.GetCartAsync(userId);
            GrandTotal = CartItems.Sum(x => x.TotalPrice);
            CartCount = CartItems.Sum(x => x.Quantity);
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(long productId, int change)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;

            // Lấy thông tin giỏ hàng hiện tại để biết số lượng cũ
            var items = await _cartService.GetCartAsync(userId);
            var item = items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                // Tính toán số lượng mới = cũ + thay đổi (1 hoặc -1)
                int newQty = item.Quantity + change;
                await _cartService.UpdateQuantityAsync(userId, productId, newQty);
            }

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(long productId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;

            await _cartService.RemoveItemAsync(userId, productId);
            return new JsonResult(new { success = true });
        }
    }
}