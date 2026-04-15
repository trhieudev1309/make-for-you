using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductRepository _productRepo;

        public DetailsModel(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        public Product? Product { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            Id = id;
            Product = await _productRepo.FindByIdAsync(id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
