using FUNews.BusinessLogic.Entities;
using FUNews.DataAccess;
using FUNews.Repositories.Interfaces;

namespace FUNews.Repositories.Repository
{
    public class SystemAccountRepository : ISystemAccountRepository
    {
        private readonly SystemAccountDAO _dao;

        public SystemAccountRepository(SystemAccountDAO dao)
        {
            _dao = dao;
        }

        public SystemAccount? GetById(int accountId)
            => _dao.GetById(accountId);

        public IEnumerable<SystemAccount> GetAll()
            => _dao.GetAll();

        public void Add(SystemAccount account)
            => _dao.Add(account);

        public void Update(SystemAccount account)
            => _dao.Update(account);

        public void Delete(short accountId)
            => _dao.Delete(accountId);

        public SystemAccount? GetByEmail(string email)
            => _dao.GetByEmail(email);

        public bool IsEmailExists(string email, int? excludeAccountId = null)
            => _dao.IsEmailExists(email, excludeAccountId);

        public IEnumerable<SystemAccount> Search(string keyword)
            => _dao.Search(keyword);

        public IEnumerable<SystemAccount> GetByRole(int role)
            => _dao.GetByRole(role);

        public bool HasCreatedArticles(int accountId)
            => _dao.HasCreatedArticles(accountId);
    }
}
