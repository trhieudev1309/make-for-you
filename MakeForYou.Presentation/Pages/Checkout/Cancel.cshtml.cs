using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Checkout
{
    public class CancelModel : PageModel
    {
        public long OrderCode { get; private set; }

        public IActionResult OnGet(long orderCode)
        {
            OrderCode = orderCode;
            return Page();
        }
    }
}
