using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductRepository _productRepo;
        private readonly ICartService _cartService;

        public DetailsModel(IProductRepository productRepo, ICartService cartService)
        {
            _productRepo = productRepo;
            _cartService = cartService;
        }

        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        public Product? Product { get; set; }
        public int CartCount { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            Id = id;
            Product = await _productRepo.FindByIdAsync(id);

            if (Product == null)
            {
                return NotFound();
            }

            // Lấy số lượng giỏ hàng hiện tại để hiển thị trên Header
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;
            CartCount = await _cartService.GetTotalItemsCountAsync(userId);

            return Page();
        }

        // Xử lý AJAX Add to Cart
        public async Task<IActionResult> OnPostAddToCartAsync(long productId, int quantity)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;

                await _cartService.AddToCartAsync(userId, productId, quantity);

                // Trả về số lượng mới để JS cập nhật Badge
                var newCount = await _cartService.GetTotalItemsCountAsync(userId);

                return new JsonResult(new { success = true, newCount = newCount });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "Lỗi khi thêm vào giỏ hàng." });
            }
        }
    }
}