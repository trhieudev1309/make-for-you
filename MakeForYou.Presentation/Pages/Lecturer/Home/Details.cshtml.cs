using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Lecturer.Home
{
    public class ArticleModel : PageModel
    {
        private readonly INewsArticleRepository _newsRepo;

        public ArticleModel(INewsArticleRepository newsRepo)
        {
            _newsRepo = newsRepo;
        }

        public NewsArticle? Article { get; set; }

        public IEnumerable<NewsArticle> RelatedArticles { get; set; }
            = new List<NewsArticle>();

        public IActionResult OnGet(int id)
        {
            Article = _newsRepo.GetById(id.ToString());

            if (Article == null || !Article.NewsStatus)
                return RedirectToPage("./Index");

            // Related = same category, different article
            RelatedArticles = _newsRepo.GetRelatedArticles(id.ToString());

            return Page();
        }
    }
}