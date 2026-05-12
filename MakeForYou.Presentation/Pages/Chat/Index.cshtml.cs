using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Chat
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public string CurrentUserId { get; private set; } = string.Empty;

        public void OnGet()
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}
