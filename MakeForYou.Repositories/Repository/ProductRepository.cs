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
                                      || p.Description.ToLower().Contains(search));
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
                .FirstOrDefaultAsync(s => s.SellerId == id);
        }
    }

    }
