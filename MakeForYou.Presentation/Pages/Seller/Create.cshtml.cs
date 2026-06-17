using System.Text.Json;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize]
    public class CreateProductModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICustomizationService _customizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public CreateProductModel(
            IProductService productService,
            ICustomizationService customizationService,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _productService = productService;
            _customizationService = customizationService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [BindProperty]
        public ProductRequest FormInput { get; set; } = new();

        [BindProperty]
        public IFormFile? UploadedImage { get; set; }

        [BindProperty]
        public string? CustomizationsJson { get; set; }

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
                await LoadCategoriesAsync();
                return Page();
            }

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out var sellerId))
                return RedirectToPage("/Auth/Login");

            FormInput.SellerId = sellerId;
            FormInput.Status = 1;

            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + "_" + Path.GetFileName(UploadedImage.FileName);
                string filePath = Path.Combine(folder, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                    await UploadedImage.CopyToAsync(fs);
                FormInput.ImageUrl = "/uploads/products/" + fileName;
            }

            var isSuccess = await _productService.CreateProductAsync(FormInput);
            if (!isSuccess)
            {
                ModelState.AddModelError(string.Empty, "Lỗi xảy ra trong quá trình lưu sản phẩm.");
                await LoadCategoriesAsync();
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(CustomizationsJson))
            {
                var created = await _context.Products
                    .Where(p => p.SellerId == sellerId && p.Title == FormInput.Title)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (created != null)
                {
                    try
                    {
                        var groups = JsonSerializer.Deserialize<List<CustomizationGroupRequest>>(CustomizationsJson);
                        if (groups != null && groups.Count > 0)
                            await _customizationService.CreateCustomizationGroupsAsync(created.ProductId, groups);
                    }
                    catch { /* ignore serialization errors */ }
                }
            }

            return RedirectToPage("/Seller/Profile");
        }

        private async Task LoadCategoriesAsync()
        {
            CategoryOptions = await _context.Categories
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}
