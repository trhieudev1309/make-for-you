using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.Presentation.Pages.Products
{
    public class BrowseModel : PageModel
    {
        private readonly IProductRepository _productRepo;
        public BrowseModel(IProductRepository productRepo) { _productRepo = productRepo; }

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public long? CategoryId { get; set; }

        // --- THÊM BI?N NÀY ?? NH?N KI?U S?P X?P ---
        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _productRepo.GetCategoriesAsync();

            // 1. L?y k?t qu? tìm ki?m thô
            var results = await _productRepo.SearchAsync(SearchTerm, CategoryId);

            // 2. Th?c hi?n s?p x?p d?a trên SortOrder
            Products = SortOrder switch
            {
                "price_asc" => results.OrderBy(p => p.Price).ToList(),
                "price_desc" => results.OrderByDescending(p => p.Price).ToList(),
                "name_asc" => results.OrderBy(p => p.Title).ToList(),
                _ => results.OrderByDescending(p => p.ProductId).ToList() // M?c ??nh m?i nh?t
            };
        }
    }
}