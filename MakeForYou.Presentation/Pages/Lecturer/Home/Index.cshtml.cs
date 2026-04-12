using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Lecturer.Home
{
    public class IndexModel : PageModel
    {
        private readonly INewsArticleRepository _newsRepo;

        public IndexModel(INewsArticleRepository newsRepo)
        {
            _newsRepo = newsRepo;
        }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public IEnumerable<NewsArticle> Articles { get; set; }
            = new List<NewsArticle>();

        public void OnGet()
        {
            var query = _newsRepo
                .GetAll()
                .Where(a => a.NewsStatus); // only active

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                query = query.Where(a =>
                    a.NewsTitle.Contains(Keyword) ||
                    a.Headline.Contains(Keyword));
            }

            Articles = query
                .OrderByDescending(a => a.CreatedDate)
                .ToList();
        }
    }
}