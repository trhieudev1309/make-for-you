using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
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
        private readonly ICustomizationService _customizationService;

        public DetailsModel(IProductRepository productRepo, ICartService cartService, ICustomizationService customizationService)
        {
            _productRepo = productRepo;
            _cartService = cartService;
            _customizationService = customizationService;
        }

        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        public Product? Product { get; set; }
        public int CartCount { get; set; }
        public int SoldCount { get; set; }

        // danh sách sản phẩm liên quan / gợi ý
        public List<Product> RelatedProducts { get; set; } = new();

        // nhận xét (kèm ảnh) của sản phẩm này
        public List<Review> Reviews { get; set; } = new();

        // Customizations for this product
        public List<CustomizationGroupViewModel> Customizations { get; set; } = new();

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

            // Tính số đã bán (x10, nếu bằng 0 thì random 10-30)
            var rawCount = await _productRepo.GetSoldCountAsync(id);
            if (rawCount == 0)
            {
                var rng = new Random((int)(id & 0x7FFFFFFF));
                SoldCount = rng.Next(10, 31);
            }
            else
            {
                SoldCount = rawCount * 10;
            }

            // Lấy sản phẩm liên quan bằng phương thức chuyên dụng
            try
            {
                RelatedProducts = await _productRepo.GetRelatedProductsAsync(id, 4);
            }
            catch
            {
                RelatedProducts = new List<Product>();
            }

            // Lấy nhận xét của sản phẩm này
            try
            {
                Reviews = await _productRepo.GetProductReviewsAsync(id, 4);
            }
            catch
            {
                Reviews = new List<Review>();
            }

            // Load customizations for this product
            try
            {
                Customizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(id);
            }
            catch
            {
                Customizations = new List<CustomizationGroupViewModel>();
            }

            return Page();
        }

        // Xử lý AJAX Add to Cart (with customizations)
        public async Task<IActionResult> OnPostAddToCartAsync([FromBody] AddToCartRequest request)
        {
            try
            {
                var productId = request.ProductId;
                var quantity = request.Quantity;
                var customizationsJson = request.CustomizationJson;
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;

                // Add to cart with customizations
                await _cartService.AddToCartAsync(userId, productId, quantity, customizationsJson);

                // Trả về số lượng mới để JS cập nhật Badge
                var newCount = await _cartService.GetTotalItemsCountAsync(userId);

                return new JsonResult(new { success = true, newCount = newCount });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Lỗi khi thêm vào giỏ hàng: " + ex.Message });
            }
        }
    }
}