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
        public List<Seller> Sellers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(long id)
        {
            // Tìm sản phẩm trong DB
            var p = await _context.Products.FindAsync(id);
            if (p == null) return RedirectToPage("/Admin/Products");

            ProductId = p.ProductId;

            // Gán dữ liệu sang Model để hiển thị lên Form
            Product = new ProductRequest
            {
                Title = p.Title,
                // FIX LỖI NULL: Nếu DB là null, gán thành chuỗi rỗng để ô nhập không hiện chữ "null"
                Description = p.Description ?? string.Empty,
                Price = p.Price ?? 0,
                ImageUrl = p.ImageUrl ?? string.Empty,
                CategoryId = p.CategoryId ?? 0,
                SellerId = p.SellerId,
                Status = p.Status
            };

            // Load danh sách để chọn lại nếu cần
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