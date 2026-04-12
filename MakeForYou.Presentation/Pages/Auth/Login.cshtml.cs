using FUNews.Presentation.Constants;
using FUNews.Presentation.Models;
using FUNews.Presentation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;

        public LoginModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginViewModel Login { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var account = _authService.Authenticate(
                Login.Email,
                Login.Password);

            if (account == null)
            {
                Login.ErrorMessage = "Invalid email or password";
                return Page();
            }

            HttpContext.Session.SetInt32("AccountId", account.AccountId);
            HttpContext.Session.SetString("AccountName", account.AccountName!);
            HttpContext.Session.SetInt32("AccountRole", account.AccountRole ?? 0);
            HttpContext.Session.SetString(
                "AccountRoleString",
                AccountRoles.GetRoleByRoleId(account.AccountRole ?? 1));

            // Razor Pages redirects
            if (account.AccountRole == AccountRoles.Admin)
                return RedirectToPage("/Admin/Home/Index");

            if (account.AccountRole == AccountRoles.Lecturer)
                return RedirectToPage("/Lecturer/Home/Index");

            if (account.AccountRole == AccountRoles.Staff)
                return RedirectToPage("/Staff/Home/Index");

            return RedirectToPage("/Reporter/Home/Index");
        }

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage();
        }
    }
}