using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Admin.Reports
{
    public class IndexModel : PageModel
    {
        private readonly INewsArticleRepository _newsRepo;

        public IndexModel(INewsArticleRepository newsRepo)
        {
            _newsRepo = newsRepo;
        }

        // Bind query parameters
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // Strongly typed properties (instead of ViewBag)
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public Dictionary<string, int> ByCategory { get; set; } = new();
        public Dictionary<string, int> ByAuthor { get; set; } = new();

        public void OnGet()
        {
            DateTime from = StartDate ?? DateTime.Today.AddMonths(-1);
            DateTime to = EndDate ?? DateTime.Today;

            ActiveCount = _newsRepo.CountByStatus(true, from, to);
            InactiveCount = _newsRepo.CountByStatus(false, from, to);
            ByCategory = _newsRepo.CountByCategory(from, to).ToDictionary();
            ByAuthor = _newsRepo.CountByAuthor(from, to).ToDictionary();

            // Normalize back to date inputs
            StartDate = from;
            EndDate = to;
        }
    }
}
