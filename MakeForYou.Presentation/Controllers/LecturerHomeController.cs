using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FUNews.Presentation.Controllers
{
    [Route("Lecturer")]
    public class LecturerHomeController : Controller
    {
        private readonly INewsArticleRepository _newsRepo;

        public LecturerHomeController(INewsArticleRepository newsRepo)
        {
            _newsRepo = newsRepo;
        }

        // HOME: search + list (published only)
        [HttpGet("")]
        public IActionResult Index(string? keyword)
        {
            IEnumerable<NewsArticle> articles;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                articles = _newsRepo
                    .GetAll()
                    .Where(n => n.NewsStatus == true);
            }
            else
            {
                articles = _newsRepo
                    .Search(keyword)
                    .Where(n => n.NewsStatus == true);
            }

            return View(articles);
        }

        // READ ARTICLE
        [HttpGet("Article/{id}")]
        public IActionResult Details(string id)
        {
            var article = _newsRepo.GetById(id);

            // Not found or not published → block lecturer
            if (article == null || article.NewsStatus == false)
                return NotFound();

            // Related articles (repo already limits to 3)
            ViewBag.RelatedArticles =
                _newsRepo.GetRelatedArticles(id);

            return View(article);
        }
    }
}
