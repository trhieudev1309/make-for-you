using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Checkout
{
    public class SuccessModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public long OrderId { get; set; }

        public void OnGet(long orderId)
        {
            OrderId = orderId;
        }
    }
}