using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _productRepo;

        public IndexModel(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public long? CategoryId { get; set; }

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _productRepo.GetCategoriesAsync();
            Products = await _productRepo.SearchAsync(Keyword, CategoryId);
        }
    }
}
