using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Staff.Home
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Later: add session + role check for Staff

            return Page();
        }
    }
}