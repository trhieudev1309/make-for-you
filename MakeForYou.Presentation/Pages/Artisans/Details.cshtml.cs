using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Artisans
{
    public class DetailsModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly ISellerPostService _postService;

        public DetailsModel(IProductRepository productRepository, ISellerPostService postService)
        {
            _productRepository = productRepository;
            _postService = postService;
        }

        public MakeForYou.BusinessLogic.Entities.Seller Artisan { get; set; } = null!;
        public List<Product> Products { get; set; } = new();
        public List<PortfolioItem> Portfolio { get; set; } = new();
        public List<SellerPost> Posts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(long id)
        {
            if (id <= 0) return NotFound();

            var seller = await _productRepository.GetSellerDetailsAsync(id);
            if (seller == null || seller.User == null) return NotFound();

            Artisan   = seller;
            Products  = seller.Products?.OrderByDescending(p => p.CreatedAt).ToList() ?? new();
            Portfolio = seller.PortfolioItems?.OrderByDescending(p => p.CreatedAt).ToList() ?? new();
            Posts     = await _postService.GetPostsBySellerAsync(id);

            return Page();
        }

        public string GetMonogram()
        {
            var name = Artisan.User.FullName?.Trim();
            if (string.IsNullOrEmpty(name)) return "A";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : parts[0][0].ToString().ToUpper();
        }

        public string FormatPrice(decimal? price) =>
            price.HasValue ? price.Value.ToString("N0") + " VNĐ" : "—";
    }
}
