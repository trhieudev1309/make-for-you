using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context) => _context = context;

        // Lấy toàn bộ sản phẩm cho Admin (Task 27)
        public async Task<List<ProductViewModel>> GetAllProductsForAdminAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Title = p.Title ?? "No Title",
                    // Fix lỗi CS0266: Xử lý int? sang int
                    Price = p.Price ?? 0,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    IsAvailable = p.Status == 1
                }).ToListAsync();
        }

        // Lấy sản phẩm nổi bật cho trang chủ
        public async Task<List<ProductViewModel>> GetFeaturedProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == 1)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Title = p.Title ?? "No Title",
                    Price = p.Price ?? 0,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    IsAvailable = true
                }).ToListAsync();
        }

        // Tạo sản phẩm mới
        public async Task<bool> CreateProductAsync(ProductRequest req)
        {
            var product = new Product
            {
                Title = req.Title,
                Description = req.Description,
                Price = req.Price,
                ImageUrl = req.ImageUrl,
                CategoryId = req.CategoryId,
                SellerId = req.SellerId, // Phải có SellerId vì Entity yêu cầu [Required]
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(product);
            return await _context.SaveChangesAsync() > 0;
        }

        // Cập nhật sản phẩm
        public async Task<bool> UpdateProductAsync(long id, ProductRequest req)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Title = req.Title;
            product.Description = req.Description;
            product.Price = req.Price;
            product.ImageUrl = req.ImageUrl;
            product.CategoryId = req.CategoryId;
            product.Status = req.Status;

            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        // Xóa sản phẩm (Soft Delete)
        public async Task<bool> DeleteProductAsync(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Status = 0;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}