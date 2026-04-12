using FUNews.BusinessLogic.Entities;
using FUNews.Presentation.Constants;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Admin.SystemAccounts
{
    public class IndexModel : PageModel
    {
        private readonly ISystemAccountRepository _repository;

        public IndexModel(ISystemAccountRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<SystemAccount> Accounts { get; set; } = new List<SystemAccount>();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Role { get; set; }
        [BindProperty]
        public Category EditCategory { get; set; }

        public List<Category> AllCategories { get; set; } = new();

        public bool IsUsed { get; set; }
        public void OnGet()
        {
            var accounts = _repository.GetAll();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                accounts = accounts.Where(a =>
                    a.AccountName.Contains(Keyword) ||
                    a.AccountEmail.Contains(Keyword) ||
                    AccountRoles.GetRoleByRoleId(a.AccountRole).Contains(Keyword)
                );
            }

            if (Role.HasValue)
            {
                accounts = accounts.Where(a => a.AccountRole == Role);
            }

            Accounts = accounts;
        }

        // ================= CREATE =================

        public IActionResult OnGetCreate()
        {
            return Partial("Modals/_CreateModal", new SystemAccount());
        }

        public IActionResult OnPostCreate(SystemAccount account, string password)
        {
            if (_repository.IsEmailExists(account.AccountEmail))
            {
                ModelState.AddModelError("AccountEmail", "Email already exists.");
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("password", "Password must be at least 6 characters");
            }
            else if (!password.Any(char.IsDigit))
            {
                ModelState.AddModelError("password", "Password must contain at least one number");
            }

            //if (!ModelState.IsValid)
            //{
            //    return Partial("Modals/_CreateFormBody", account);
            //}

            var hasher = new PasswordHasher<SystemAccount>();
            account.AccountPassword = hasher.HashPassword(account, password);

            _repository.Add(account);
            return new JsonResult(new { success = true });
        }

        // ================= EDIT =================

        public IActionResult OnGetEdit(int id)
        {
            var account = _repository.GetById(id);
            return Partial("Modals/_EditModal", account);
        }

        public IActionResult OnPostEdit(SystemAccount account)
        {
            if (_repository.IsEmailExists(account.AccountEmail, account.AccountId))
            {
                ModelState.AddModelError("AccountEmail", "Email already exists.");
            }

            if (!ModelState.IsValid)
            {
                return Partial("Modals/_EditModal", account);
            }

            _repository.Update(account);
            return new JsonResult(new { success = true });
        }

        // ================= CHANGE PASSWORD =================

        public IActionResult OnGetChangePassword(int id)
        {
            return Partial("Modals/_ChangePasswordModal", id);
        }

        public IActionResult OnPostChangePassword(
            int accountId,
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                return new JsonResult(new { success = false, message = "Passwords do not match." });
            }

            if (newPassword.Length < 6 || !newPassword.Any(char.IsDigit))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Password must be at least 6 characters and contain a number"
                });
            }

            var account = _repository.GetById(accountId);
            var hasher = new PasswordHasher<SystemAccount>();

            var result = hasher.VerifyHashedPassword(
                account,
                account.AccountPassword,
                currentPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                return new JsonResult(new { success = false, message = "Current password is incorrect." });
            }

            account.AccountPassword = hasher.HashPassword(account, newPassword);
            _repository.Update(account);

            return new JsonResult(new { success = true });
        }

        // ================= DELETE =================

        public IActionResult OnGetDelete(short id)
        {
            if (_repository.HasCreatedArticles(id))
            {
                TempData["Error"] = "Cannot delete account that has created news articles.";
                return RedirectToPage();
            }

            _repository.Delete(id);
            return RedirectToPage();
        }
    }
}