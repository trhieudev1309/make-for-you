using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities; // Chứa DbContext và Entity Seller
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
        private readonly ApplicationDbContext _context;

        public CreateModel(IProductService productService,
                          ICategoryService categoryService,
                          ApplicationDbContext context)
        {
            _productService = productService;
            _categoryService = categoryService;
            _context = context;
        }

        [BindProperty]
        public ProductRequest Product { get; set; } = new();

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

            if (result) return RedirectToPage("/Admin/Index");

            await OnGetAsync();
            return Page();
        }
    }
}