using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Admin.Audit
{
    public class IndexModel : PageModel
    {
        private readonly INewsArticleRepository _newsRepo;
        private readonly ISystemAccountRepository _accountRepo;

        public IndexModel(
            INewsArticleRepository newsRepo,
            ISystemAccountRepository accountRepo)
        {
            _newsRepo = newsRepo;
            _accountRepo = accountRepo;
        }

        public List<NewsArticle> Articles { get; set; } = new();
        public List<SystemAccount> Accounts { get; set; } = new();

        public void OnGet()
        {
            Articles = _newsRepo
                .GetAll()
                .OrderByDescending(a => a.ModifiedDate ?? a.CreatedDate)
                .ToList();

            Accounts = _accountRepo.GetAll().ToList();
        }
    }
}