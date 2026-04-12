using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Staff.Tags
{
    public class ArticlesModel : PageModel
    {
        private readonly ITagRepository _tagRepo;
        private readonly INewsArticleRepository _newsRepo;

        public ArticlesModel(
            ITagRepository tagRepo,
            INewsArticleRepository newsRepo)
        {
            _tagRepo = tagRepo;
            _newsRepo = newsRepo;
        }

        public IEnumerable<NewsArticle> Articles { get; set; }
            = new List<NewsArticle>();

        public string? TagName { get; set; }

        public IActionResult OnGet(short id)
        {
            var tag = _tagRepo.GetById(id);

            if (tag == null)
                return RedirectToPage("./Index");

            TagName = tag.TagName;

            Articles = _newsRepo
                .GetAll()
                .Where(a => a.Tags.Any(t => t.TagId == id))
                .OrderByDescending(a => a.CreatedDate)
                .ToList();

            return Page();
        }
    }
}