using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize]
    public class CreateProductModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public CreateProductModel(IProductService productService, IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [BindProperty]
        public ProductRequest FormInput { get; set; } = new();

        [BindProperty]
        public IFormFile? UploadedImage { get; set; }

        // List chứa danh sách các Category hiển thị lên UI
        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(); // Load lại danh sách nếu form lỗi
                return Page();
            }

            // 1. Lấy thông tin ID người dùng (SellerId) từ Session/Cookie đăng nhập
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out var sellerId))
            {
                return RedirectToPage("/Auth/Login");
            }
            FormInput.SellerId = sellerId;

            // 2. Xử lý lưu File vật lý nếu người dùng có tải ảnh lên
            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất tránh trùng lặp bằng Guid
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadedImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedImage.CopyToAsync(fileStream);
                }

                // Gán chuỗi đường dẫn trực tiếp vào thuộc tính ImageUrl của DTO
                FormInput.ImageUrl = "/uploads/products/" + uniqueFileName;
            }

            // 3. Gọi service thực hiện lưu database bằng logic của bạn
            var isSuccess = await _productService.CreateProductAsync(FormInput);
            if (isSuccess)
            {
                return RedirectToPage("/Products/Browse");
            }

            ModelState.AddModelError(string.Empty, "Lỗi xảy ra trong quá trình lưu sản phẩm.");
            await LoadCategoriesAsync();
            return Page();
        }

        // Hàm hỗ trợ lấy danh sách từ DB đổ vào Dropdown
        private async Task LoadCategoriesAsync()
        {
            CategoryOptions = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name // Đã sửa từ CategoryName thành Name theo đúng DB của bạn
                })
                .ToListAsync();
        }
    }
}