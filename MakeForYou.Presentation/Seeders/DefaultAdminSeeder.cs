using FUNews.BusinessLogic.Entities;
using FUNews.Presentation.Constants;
using FUNews.Presentation.Services;
using FUNews.Repositories.Interfaces;

namespace FUNews.Presentation.Seeders
{
    public static class DefaultAdminSeeder
    {
        public static void Seed(
            IConfiguration configuration,
            ISystemAccountRepository accountRepo,
            AuthService authService)
        {
            var adminEmail = configuration["DefaultAdmin:Email"];
            var adminPassword = configuration["DefaultAdmin:Password"];
            var adminName = configuration["DefaultAdmin:Name"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
                return;

            var existingAdmin = accountRepo.GetByEmail(adminEmail);
            if (existingAdmin != null)
                return;

            var admin = new SystemAccount
            {
                AccountEmail = adminEmail,
                AccountName = adminName,
                AccountRole = AccountRoles.Admin
            };

            admin.AccountPassword =
                authService.HashPassword(admin, adminPassword);

            accountRepo.Add(admin);
        }
    }
}
