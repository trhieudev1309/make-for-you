using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetFeaturedAsync(int count = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller).ThenInclude(s => s.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<Seller>> GetFeaturedSellersAsync(int count = 4)
        {
            return await _context.Sellers.Include(s => s.User).Take(count).ToListAsync();
        }

        public async Task<Product?> FindByIdAsync(long id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        // --- ĐÂY LÀ HÀM QUAN TRỌNG NHẤT ĐỂ TÌM KIẾM ---
        public async Task<List<Product>> SearchAsync(string? searchTerm, long? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller).ThenInclude(s => s.User)
                .AsQueryable();

            // Nếu có từ khóa, phải lọc gắt gao
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(search)
                                      || (p.Description != null && p.Description.ToLower().Contains(search))
                                      || (p.Seller != null && p.Seller.User != null && p.Seller.User.FullName != null && p.Seller.User.FullName.ToLower().Contains(search)));
            }

            // Nếu có chọn category, lọc đúng category đó
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Seller?> GetSellerDetailsAsync(long id)
        {
            return await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Category)
                .Include(s => s.PortfolioItems)
                .FirstOrDefaultAsync(s => s.SellerId == id);
        }

        // New: Tìm sản phẩm liên quan theo chiến lược:
        // 1) cùng danh mục (loại ưu tiên)
        // 2) nếu chưa đủ, bổ sung theo cùng nghệ nhân
        // 3) nếu vẫn chưa đủ, bổ sung sản phẩm gần đây khác
        public async Task<List<Product>> GetRelatedProductsAsync(long productId, int count)
        {
            var current = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (current == null) return new List<Product>();

            var related = new List<Product>();

            // 1) cùng danh mục
            if (current.CategoryId > 0)
            {
                var sameCategory = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Seller).ThenInclude(s => s.User)
                    .Where(p => p.ProductId != productId && p.CategoryId == current.CategoryId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .ToListAsync();

                related.AddRange(sameCategory);
            }

            // 2) bổ sung theo cùng nghệ nhân nếu cần
            if (related.Count < count && current.SellerId > 0)
            {
                var existingIds = related.Select(p => p.ProductId).Append(productId).ToList();
                var sameSeller = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Seller).ThenInclude(s => s.User)
                    .Where(p => !existingIds.Contains(p.ProductId) && p.SellerId == current.SellerId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count - related.Count)
                    .ToListAsync();

                related.AddRange(sameSeller);
            }

            // 3) nếu vẫn chưa đủ, bổ sung các sản phẩm gần đây khác
            if (related.Count < count)
            {
                var existingIds = related.Select(p => p.ProductId).Append(productId).ToList();
                var recent = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Seller).ThenInclude(s => s.User)
                    .Where(p => !existingIds.Contains(p.ProductId))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count - related.Count)
                    .ToListAsync();

                related.AddRange(recent);
            }

            // đảm bảo không vượt quá số lượng yêu cầu
            return related.Take(count).ToList();
        }
    }

}
