using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities; // Thêm để dùng ApplicationDbContext
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
        private readonly ApplicationDbContext _context; // Dùng để lấy danh sách Seller nhanh

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

        // Danh sách Seller để Admin chọn
        public List<Seller> Sellers { get; set; } = new();

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
                await OnGetAsync(); // Load lại các list nếu form lỗi
                return Page();
            }

            // Status mặc định là 1 (Đang bán) khi Admin tạo mới
            Product.Status = 1;

            // Lúc này Product.SellerId đã được gán từ Dropdown trên giao diện
            var result = await _productService.CreateProductAsync(Product);

            if (result) return RedirectToPage("/Admin/Index");

            await OnGetAsync();
            return Page();
        }
    }
}