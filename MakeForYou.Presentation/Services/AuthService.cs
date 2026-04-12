using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FUNews.Presentation.Services
{
    public class AuthService
    {
        private readonly ISystemAccountRepository _accountRepo;
        private readonly PasswordHasher<SystemAccount> _hasher;

        public AuthService(ISystemAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
            _hasher = new PasswordHasher<SystemAccount>();
        }

        public SystemAccount? Authenticate(string email, string password)
        {
            var account = _accountRepo.GetByEmail(email);
            if (account == null)
                return null;

            var result = _hasher.VerifyHashedPassword(
                account,
                account.AccountPassword,
                password
            );

            return result == PasswordVerificationResult.Success
                ? account
                : null;
        }

        public string HashPassword(SystemAccount account, string password)
        {
            return _hasher.HashPassword(account, password);
        }
    }
}
