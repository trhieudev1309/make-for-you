using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _auth;
        public LoginModel(IAuthService auth) => _auth = auth;

        [BindProperty]
        public LoginRequest Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public bool JustRegistered { get; set; }

        public IActionResult OnGet(bool registered = false)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Home/Index");

            JustRegistered = registered;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _auth.LoginAsync(Input);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            // Redirect to original page or role-based default
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return result.Role switch
            {
                (int)UserRole.Seller => RedirectToPage("/Seller/Dashboard"),
                (int)UserRole.Admin => RedirectToPage("/Admin/Dashboard"),
                _ => RedirectToPage("/Buyer/Dashboard")
            };
        }
    }
}
