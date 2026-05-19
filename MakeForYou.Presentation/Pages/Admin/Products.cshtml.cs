using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;

        public ProductsModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<ProductViewModel> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            Products = await _productService.GetAllProductsForAdminAsync();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                Products = Products.Where(p => 
                    (p.Title != null && p.Title.ToLower().Contains(term)) ||
                    (p.CategoryName != null && p.CategoryName.ToLower().Contains(term))
                ).ToList();
            }
        }


    
        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            // Gọi Service thực hiện Soft Delete (đổi Status = 0)
            var result = await _productService.DeleteProductAsync(id);

            return RedirectToPage();
        }
    }
}