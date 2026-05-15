using Microsoft.EntityFrameworkCore;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context) => _context = context;

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    ProductCount = c.Products.Count // Đếm số sản phẩm thuộc danh mục
                }).ToListAsync();
        }

        public async Task<bool> CreateCategoryAsync(string name)
        {
            _context.Categories.Add(new Category { Name = name });
            return await _context.SaveChangesAsync() > 0;
        }

        // 1. BỔ SUNG: Hàm Update để xử lý từ Modal sửa
        public async Task<bool> UpdateCategoryAsync(long id, string name)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return false;

            cat.Name = name;
            _context.Categories.Update(cat);
            return await _context.SaveChangesAsync() > 0;
        }

        // 2. CẬP NHẬT: Hàm Delete an toàn hơn
        public async Task<bool> DeleteCategoryAsync(long id)
        {
            // Kiểm tra xem danh mục có đang chứa sản phẩm không để tránh lỗi FK Constraint
            var cat = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (cat == null) return false;

            // Nếu danh mục đang có sản phẩm, chúng ta không nên xóa trực tiếp
            if (cat.Products.Any())
            {
                // Tiến có thể ném ra một Exception hoặc trả về false để báo cho Admin biết
                return false;
            }

            _context.Categories.Remove(cat);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}