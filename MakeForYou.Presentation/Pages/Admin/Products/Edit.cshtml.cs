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
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICustomizationService _customizationService;
        private readonly ApplicationDbContext _context;

        public EditModel(IProductService productService,
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
        public long ProductId { get; set; }

        [BindProperty]
        public string? CustomizationsJson { get; set; }

        public List<MakeForYou.BusinessLogic.Entities.DTOs.Respond.CategoryViewModel> Categories { get; set; } = new();

        public List<MakeForYou.BusinessLogic.Entities.Seller> Sellers { get; set; } = new();

        // Use ViewModel instead of Entity to avoid circular references
        public List<CustomizationGroupViewModel> ExistingCustomizations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return RedirectToPage("/Admin/Products");

            ProductId = p.ProductId;

            Product = new ProductRequest
            {
                Title = p.Title,
                Description = p.Description ?? string.Empty,
                Price = p.Price ?? 0,
                ImageUrl = p.ImageUrl ?? string.Empty,
                CategoryId = p.CategoryId ?? 0,
                SellerId = p.SellerId,
                Status = p.Status
            };

            Categories = await _categoryService.GetAllCategoriesAsync();
            Sellers = await _context.Sellers.ToListAsync();

            // Load existing customizations as ViewModels
            ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _categoryService.GetAllCategoriesAsync();
                Sellers = await _context.Sellers.ToListAsync();
                ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(ProductId);
                return Page();
            }

            var result = await _productService.UpdateProductAsync(ProductId, Product);
            if (!result)
            {
                Categories = await _categoryService.GetAllCategoriesAsync();
                Sellers = await _context.Sellers.ToListAsync();
                ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(ProductId);
                ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm");
                return Page();
            }

            // Delete all existing customizations for this product
            await _customizationService.DeleteAllCustomizationsByProductIdAsync(ProductId);

            // Add new customizations if provided
            if (!string.IsNullOrWhiteSpace(CustomizationsJson))
            {
                try
                {
                    var customizations = JsonSerializer.Deserialize<List<CustomizationGroupRequest>>(CustomizationsJson);

                    if (customizations != null && customizations.Count > 0)
                    {
                        await _customizationService.CreateCustomizationGroupsAsync(ProductId, customizations);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating customizations: {ex.Message}");
                }
            }

            return RedirectToPage("/Admin/Products");
        }
    }
}