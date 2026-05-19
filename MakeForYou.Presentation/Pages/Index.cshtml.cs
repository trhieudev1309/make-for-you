using System.Security.Claims;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Thư viện cần thiết để chạy LINQ

namespace MakeForYou.Presentation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHomeService _homeService;
        private readonly IProductRepository _productRepo;
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context; // Inject DbContext vào đây

        public IndexModel(
            IHomeService homeService,
            IProductRepository productRepo,
            ICartService cartService,
             ApplicationDbContext context)
        {
            _homeService = homeService;
            _productRepo = productRepo;
            _cartService = cartService;
            _context = context;
        }

        public HomeViewModel HomeData { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new(); // Danh sách lưu sản phẩm bán chạy
        public int CartCount { get; set; }

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

            // 3. LẤY DANH SÁCH BEST SELLERS (Dự phòng Mock Data nếu DB trống)
            var bestSellerIds = await _context.OrderItems
                .Where(oi => oi.Order.Status == 4) // 4 là mã int của OrderStatus.Completed
                .GroupBy(oi => oi.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(4)
                .Select(x => x.ProductId)
                .ToListAsync();

            if (bestSellerIds.Any())
            {
                // Nếu có người mua thật sự
                BestSellers = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Seller).ThenInclude(s => s.User)
                    .Where(p => bestSellerIds.Contains(p.ProductId))
                    .ToListAsync();
            }
            else
            {
                // MOCK DATA: Nếu DB chưa có đơn hàng nào, tự động lấy 4 sản phẩm bất kỳ ra hiển thị cho đẹp
                BestSellers = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Seller).ThenInclude(s => s.User)
                    .OrderBy(p => Guid.NewGuid()) // Random
                    .Take(4)
                    .ToListAsync();
            }
        }
    }
}