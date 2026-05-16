using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
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
        private readonly ApplicationDbContext _context;

        public EditModel(IProductService productService, ICategoryService categoryService, ApplicationDbContext context)
        {
            _productService = productService;
            _categoryService = categoryService;
            _context = context;
        }

        [BindProperty]
        public ProductRequest Product { get; set; } = new();

        [BindProperty]
        public long ProductId { get; set; }

        public List<MakeForYou.BusinessLogic.Entities.DTOs.Respond.CategoryViewModel> Categories { get; set; } = new();

        // SỬA TẠI ĐÂY: Chỉ định rõ namespace để tránh bị hiểu nhầm thành Namespace Seller
        public List<MakeForYou.BusinessLogic.Entities.Seller> Sellers { get; set; } = new();

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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _categoryService.GetAllCategoriesAsync();
                Sellers = await _context.Sellers.ToListAsync();
                return Page();
            }

            var result = await _productService.UpdateProductAsync(ProductId, Product);
            if (result) return RedirectToPage("/Admin/Products");

            return Page();
        }
    }
}