using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHomeService _homeService;
        private readonly IProductRepository _productRepo;

        public IndexModel(IHomeService homeService, IProductRepository productRepo)
        {
            _homeService = homeService;
            _productRepo = productRepo;
        }

        public HomeViewModel HomeData { get; set; } = new();

        // --- CÁC BIẾN NÀY SẼ GIÚP HẾT LỖI Ở FILE INDEX.CSHTML ---
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public long? CategoryId { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy danh sách Category cho Sidebar
            Categories = await _homeService.GetCategoriesAsync();
            HomeData.FeaturedArtisans = await _homeService.GetFeaturedArtisansAsync();

            // Thực hiện tìm kiếm hoặc lấy sản phẩm nổi bật
            if (!string.IsNullOrWhiteSpace(SearchTerm) || (CategoryId.HasValue && CategoryId > 0))
            {
                Products = await _productRepo.SearchAsync(SearchTerm, CategoryId);
            }
            else
            {
                Products = await _homeService.GetFeaturedProductsAsync();
            }
        }
    }
}