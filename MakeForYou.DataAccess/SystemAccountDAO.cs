using FUNews.BusinessLogic;
using FUNews.BusinessLogic.Entities;

namespace FUNews.DataAccess
{
    public class SystemAccountDAO
    {
        private readonly ApplicationDbContext _context;

        public SystemAccountDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== BASIC CRUD =====

        public SystemAccount? GetById(int accountId)
            => _context.SystemAccounts.FirstOrDefault(x => x.AccountId == accountId);

        public IEnumerable<SystemAccount> GetAll()
            => _context.SystemAccounts.ToList();

        public void Add(SystemAccount account)
        {
            var maxi = _context.SystemAccounts.Select(x => x.AccountId).Max();
            maxi += 1;
            account.AccountId = maxi;
            _context.SystemAccounts.Add(account);
            _context.SaveChanges();
        }

        public void Update(SystemAccount account)
        {
            _context.SystemAccounts.Update(account);
            _context.SaveChanges();
        }

        public void Delete(short accountId)
        {
            var account = _context.SystemAccounts.Find(accountId);
            if (account != null)
            {
                _context.SystemAccounts.Remove(account);
                _context.SaveChanges();
            }
        }

        // ===== AUTHENTICATION =====

        public SystemAccount? GetByEmail(string email)
            => _context.SystemAccounts
                .FirstOrDefault(a => a.AccountEmail == email);

        // ===== VALIDATION =====

        public bool IsEmailExists(string email, int? excludeAccountId = null)
        {
            return _context.SystemAccounts.Any(a =>
                a.AccountEmail == email &&
                (!excludeAccountId.HasValue || a.AccountId != excludeAccountId));
        }

        // ===== SEARCH & FILTER =====

        public IEnumerable<SystemAccount> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            return _context.SystemAccounts
                .Where(a =>
                    a.AccountName.Contains(keyword) ||
                    a.AccountEmail.Contains(keyword))
                .ToList();
        }

        public IEnumerable<SystemAccount> GetByRole(int role)
            => _context.SystemAccounts
                .Where(a => a.AccountRole == role)
                .ToList();

        // ===== DELETE CONSTRAINT =====

        public bool HasCreatedArticles(int accountId)
        {
            return _context.NewsArticles
                .Any(n => n.CreatedById == accountId);
        }
    }
}
