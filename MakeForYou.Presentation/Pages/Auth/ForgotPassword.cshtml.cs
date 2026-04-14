using MakeForYou.BusinessLogic.Entities.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Services;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IAuthService _auth;
        public ForgotPasswordModel(IAuthService auth) => _auth = auth;

        [BindProperty]
        public ForgotPasswordRequest Input { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Home/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _auth.ForgotPasswordAsync(Input);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            SuccessMessage = result.Message;
            return Page();
        }
    }
}