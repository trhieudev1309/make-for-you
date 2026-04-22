using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MakeForYou.BusinessLogic.Entities;
using System.Security.Claims; // Thêm cái này
using MakeForYou.BusinessLogic.Services.Interfaces; // Thêm cái này

namespace MakeForYou.Presentation.Pages.Products
{
    public class BrowseModel : PageModel
    {
        private readonly IProductRepository _productRepo;
        private readonly ICartService _cartService; // 1. Khai báo service

        public BrowseModel(IProductRepository productRepo, ICartService cartService)
        {
            _productRepo = productRepo;
            _cartService = cartService; // 2. Inject service
        }

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int CartCount { get; set; } // 3. Biến để hiện Badge

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public long? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _productRepo.GetCategoriesAsync();
            var results = await _productRepo.SearchAsync(SearchTerm, CategoryId);

            Products = SortOrder switch
            {
                "price_asc" => results.OrderBy(p => p.Price).ToList(),
                "price_desc" => results.OrderByDescending(p => p.Price).ToList(),
                "name_asc" => results.OrderBy(p => p.Title).ToList(),
                _ => results.OrderByDescending(p => p.ProductId).ToList()
            };

            // 4. LẤY SỐ LƯỢNG GIỎ HÀNG THỰC TẾ
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long? userId = !string.IsNullOrEmpty(userIdStr) ? long.Parse(userIdStr) : null;
            CartCount = await _cartService.GetTotalItemsCountAsync(userId);
        }
    }
}