using System.Security.Claims; // Cần cái này để lấy UserId
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHomeService _homeService;
        private readonly IProductRepository _productRepo;
        private readonly ICartService _cartService; // Tiêm thêm Service này

        public IndexModel(IHomeService homeService, IProductRepository productRepo, ICartService cartService)
        {
            _homeService = homeService;
            _productRepo = productRepo;
            _cartService = cartService;
        }

        public HomeViewModel HomeData { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int CartCount { get; set; } // Biến để hiển thị trên Header

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public long? CategoryId { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Lấy dữ liệu sản phẩm và danh mục
            Categories = await _homeService.GetCategoriesAsync();
            HomeData.FeaturedArtisans = await _homeService.GetFeaturedArtisansAsync();

            if (!string.IsNullOrWhiteSpace(SearchTerm) || (CategoryId.HasValue && CategoryId > 0))
            {
                Products = await _productRepo.SearchAsync(SearchTerm, CategoryId);
            }
            else
            {
                Products = await _homeService.GetFeaturedProductsAsync();
            }

            // 2. LẤY SỐ LƯỢNG GIỎ HÀNG THỰC TẾ
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;
            CartCount = await _cartService.GetTotalItemsCountAsync(userId);
        }
    }
}