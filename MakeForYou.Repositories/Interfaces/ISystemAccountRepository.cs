using FUNews.BusinessLogic.Entities;

namespace FUNews.Repositories.Interfaces
{
    public interface ISystemAccountRepository
    {
        SystemAccount? GetById(int accountId);
        IEnumerable<SystemAccount> GetAll();

        void Add(SystemAccount account);
        void Update(SystemAccount account);
        void Delete(short accountId);

        SystemAccount? GetByEmail(string email);

        bool IsEmailExists(string email, int? excludeAccountId = null);

        IEnumerable<SystemAccount> Search(string keyword);
        IEnumerable<SystemAccount> GetByRole(int role);

        bool HasCreatedArticles(int accountId);
    }
}
