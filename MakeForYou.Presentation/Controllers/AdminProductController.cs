using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakeForYou.BusinessLogic.DTOs.Request;
using MakeForYou.BusinessLogic; // Namespace chứa ApplicationDbContext của bạn
using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    public class AdminProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Tiêm DbContext vào để tương tác trực tiếp với Database
        public AdminProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// API cập nhật thông số kích thước, cân nặng cho một sản phẩm (Dành cho Admin/Seller)
        /// </summary>
        [HttpPut("{productId}/dimensions")]
        public async Task<IActionResult> UpdateDimensions(long productId, [FromBody] UpdateDimensionsRequest request)
        {
            // 1. Tìm sản phẩm trong DB bằng ProductId truyền từ URL
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound(new { message = $"Không tìm thấy sản phẩm với ID: {productId}" });
            }

            // 2. Gán dữ liệu thực tế từ Request đè lên giá trị mặc định
            product.Weight = request.Weight;
            product.Length = request.Length;
            product.Width = request.Width;
            product.Height = request.Height;

            // 3. Lưu thay đổi xuống SQL Server
            await _context.SaveChangesAsync();

            // 4. Trả về thông báo thành công kèm data mới cập nhật
            // SỬA LẠI ĐOẠN TRẢ VỀ Ở CUỐI FILE ADMINPRODUCTCONTROLLER.CS:
            return Ok(new
            {
                message = $"Đã cập nhật kích thước vật lý cho sản phẩm '{product.Title}' thành công!", // Đổi thành product.Title
                updatedData = new
                {
                    product.ProductId,
                    product.Title, // Đổi thành product.Title
                    product.Weight,
                    product.Length,
                    product.Width,
                    product.Height
                }
            });
        }
    }
}