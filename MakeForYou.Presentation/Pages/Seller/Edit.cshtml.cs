using System.Text.Json;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class SellerEditProductModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICustomizationService _customizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public SellerEditProductModel(
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

        [BindProperty] public ProductRequest Product { get; set; } = new();
        [BindProperty] public long ProductId { get; set; }
        [BindProperty] public string? CustomizationsJson { get; set; }
        [BindProperty] public IFormFile? UploadedImage { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<CustomizationGroupViewModel> ExistingCustomizations { get; set; } = new();
        public string? CurrentImageUrl { get; set; }

        private long GetSellerId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var sellerId = GetSellerId();
            var p = await _context.Products.FindAsync(id);
            if (p == null || p.SellerId != sellerId)
                return RedirectToPage("/Seller/Profile");

            ProductId = id;
            CurrentImageUrl = p.ImageUrl;
            Product = new ProductRequest
            {
                Title       = p.Title,
                Description = p.Description ?? string.Empty,
                Price       = p.Price ?? 0,
                ImageUrl    = p.ImageUrl ?? string.Empty,
                CategoryId  = p.CategoryId ?? 0,
                SellerId    = sellerId,
                Status      = p.Status,
                Weight      = p.Weight,
                Length      = p.Length,
                Width       = p.Width,
                Height      = p.Height
            };

            await LoadCategoriesAsync();
            ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var sellerId = GetSellerId();
            var existing = await _context.Products.FindAsync(ProductId);
            if (existing == null || existing.SellerId != sellerId)
                return RedirectToPage("/Seller/Profile");

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(ProductId);
                CurrentImageUrl = existing.ImageUrl;
                return Page();
            }

            Product.SellerId = sellerId;

            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + "_" + Path.GetFileName(UploadedImage.FileName);
                using (var fs = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                    await UploadedImage.CopyToAsync(fs);
                Product.ImageUrl = "/uploads/products/" + fileName;
            }
            else
            {
                Product.ImageUrl = existing.ImageUrl ?? string.Empty;
            }

            var ok = await _productService.UpdateProductAsync(ProductId, Product);
            if (!ok)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm.");
                await LoadCategoriesAsync();
                ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(ProductId);
                CurrentImageUrl = existing.ImageUrl;
                return Page();
            }

            await _customizationService.DeleteAllCustomizationsByProductIdAsync(ProductId);
            if (!string.IsNullOrWhiteSpace(CustomizationsJson))
            {
                try
                {
                    var groups = JsonSerializer.Deserialize<List<CustomizationGroupRequest>>(CustomizationsJson);
                    if (groups != null && groups.Count > 0)
                        await _customizationService.CreateCustomizationGroupsAsync(ProductId, groups);
                }
                catch { /* ignore */ }
            }

            TempData["Success"] = "Cập nhật sản phẩm thành công.";
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
