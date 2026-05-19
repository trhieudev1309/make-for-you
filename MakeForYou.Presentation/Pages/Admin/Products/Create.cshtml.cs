using System.Text.Json;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Presentation.Pages.Admin.Products
{
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICustomizationService _customizationService;
        private readonly ApplicationDbContext _context;

        public CreateModel(IProductService productService,
                          ICategoryService categoryService,
                          ICustomizationService customizationService,
                          ApplicationDbContext context)
        {
            _productService = productService;
            _categoryService = categoryService;
            _customizationService = customizationService;
            _context = context;
        }

        [BindProperty]
        public ProductRequest Product { get; set; } = new();

        [BindProperty]
        public string? CustomizationsJson { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new();

        // SỬA TẠI ĐÂY: Chỉ định rõ ràng Class Seller từ namespace Entities để tránh trùng tên với Namespace khác
        public List<MakeForYou.BusinessLogic.Entities.Seller> Sellers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllCategoriesAsync();

            // Lấy danh sách Seller đang hoạt động để gán sản phẩm
            Sellers = await _context.Sellers.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            Product.Status = 1;

            var result = await _productService.CreateProductAsync(Product);

            if (!result)
            {
                await OnGetAsync();
                return Page();
            }

            // Get the created product to retrieve its ID
            var createdProduct = await _context.Products
                .Where(p => p.SellerId == Product.SellerId && p.Title == Product.Title)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (createdProduct != null && !string.IsNullOrWhiteSpace(CustomizationsJson))
            {
                try
                {
                    // Deserialize customizations from JSON
                    var customizations = JsonSerializer.Deserialize<List<CustomizationGroupRequest>>(CustomizationsJson);

                    if (customizations != null && customizations.Count > 0)
                    {
                        await _customizationService.CreateCustomizationGroupsAsync(createdProduct.ProductId, customizations);
                    }
                }
                catch
                {
                    // Log error if needed, but don't fail the product creation
                }
            }

            return RedirectToPage("/Admin/Index");
        }
    }
}