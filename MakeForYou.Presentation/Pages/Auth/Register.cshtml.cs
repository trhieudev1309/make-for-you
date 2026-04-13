using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Services;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _auth;

        public RegisterModel(IAuthService auth) => _auth = auth;

        [BindProperty]
        public RegisterRequest Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            // Redirect if already logged in
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Home/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _auth.RegisterAsync(Input);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            SuccessMessage = result.Message;
            // Redirect to login after short delay via meta-refresh,
            // or just redirect immediately:
            return RedirectToPage("/Auth/Login", new { registered = true });
        }
    }
}