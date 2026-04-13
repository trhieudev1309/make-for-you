using MakeForYou.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.BusinessLogic.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> SearchAsync(string? keyword, long? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var pattern = $"%{keyword.Trim()}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Title ?? string.Empty, pattern) ||
                    EF.Functions.Like(p.Description ?? string.Empty, pattern));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            return await query.ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}