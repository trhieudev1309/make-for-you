using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Auth
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IAuthService _auth;
        public ResetPasswordModel(IAuthService auth) => _auth = auth;

        [BindProperty]
        public ResetPasswordRequest Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return RedirectToPage("/Auth/ForgotPassword");

            Input.Token = token;
            Input.Email = email;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _auth.ResetPasswordAsync(Input);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            return RedirectToPage("/Auth/Login", new { reset = true });
        }
    }
}
