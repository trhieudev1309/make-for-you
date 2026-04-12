using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Staff.NewsArticles
{
    public class IndexModel : PageModel
    {
        private readonly INewsArticleRepository _newsRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly ITagRepository _tagRepo;

        public IndexModel(
            INewsArticleRepository newsRepo,
            ICategoryRepository categoryRepo,
            ITagRepository tagRepo)
        {
            _newsRepo = newsRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
        }

        public IEnumerable<NewsArticle> Articles { get; set; }
            = new List<NewsArticle>();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty]
        public NewsArticle EditArticle { get; set; }

        [BindProperty]
        public List<int>? TagIds { get; set; }

        [BindProperty]
        public NewsArticle CreateArticle { get; set; } = new();

        [BindProperty]
        public List<int>? CreateTagIds { get; set; }

        public IEnumerable<Category> Categories { get; set; }
            = new List<Category>();

        public IEnumerable<Tag> Tags { get; set; }
            = new List<Tag>();

        // ================== LIST ==================
        public void OnGet()
        {
            if (string.IsNullOrWhiteSpace(Keyword))
            {
                Articles = _newsRepo
                    .GetAll().OrderBy(x => x.CreatedDate);
            }
            else
            {
                Articles = _newsRepo
                    .Search(Keyword).OrderBy(x => x.CreatedDate)
                    ;
            }
        }

        // ================== DETAILS ==================
        public IActionResult OnGetDetails(string id)
        {
            var article = _newsRepo.GetById(id);

            if (article == null || article.NewsStatus == false)
                return NotFound();

            ViewData["RelatedArticles"] =
                _newsRepo.GetRelatedArticles(id);

            return Page();
        }

        // ================== EDIT GET ==================
        public IActionResult OnGetEdit(string id)
        {
            var article = _newsRepo.GetById(id);

            if (article == null)
                return NotFound();

            EditArticle = article;

            Categories = _categoryRepo.GetAll();
            Tags = _tagRepo.GetAll();

            return Partial("Modals/_EditModal", this);
        }

        // ================== EDIT POST ==================
        public IActionResult OnPostEdit()
        {
            var existing = _newsRepo.GetById(EditArticle.NewsArticleId);

            if (existing == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(EditArticle.Headline))
            {
                ModelState.AddModelError(
                    "EditArticle.Headline",
                    "Headline is required");
            }

            //if (!ModelState.IsValid)
            //{
            //    Categories = _categoryRepo.GetAll();
            //    Tags = _tagRepo.GetAll();
            //    return Partial("Modals/_EditModal", this);
            //}

            // ===== UPDATE FIELDS =====
            existing.NewsTitle = EditArticle.NewsTitle;
            existing.Headline = EditArticle.Headline;
            existing.CategoryId = EditArticle.CategoryId;
            existing.NewsContent = EditArticle.NewsContent;
            existing.NewsSource = EditArticle.NewsSource;
            existing.NewsStatus = EditArticle.NewsStatus;
            existing.ModifiedDate = DateTime.Now;

            // ===== SYNC TAGS =====
            TagIds ??= new List<int>();

            var existingTagIds = existing.Tags
                .Select(t => t.TagId)
                .ToList();

            // Remove unchecked
            foreach (var oldTagId in existingTagIds)
            {
                if (!TagIds.Contains(oldTagId))
                {
                    _newsRepo.RemoveTag(existing.NewsArticleId, oldTagId);
                }
            }

            // Add new
            foreach (var newTagId in TagIds)
            {
                if (!existingTagIds.Contains(newTagId))
                {
                    _newsRepo.AddTag(existing.NewsArticleId, newTagId);
                }
            }

            _newsRepo.Update(existing);

            return new JsonResult(new { success = true });
        }
        public IActionResult OnGetCreate()
        {
            CreateArticle = new NewsArticle
            {
                NewsStatus = false
            };

            Categories = _categoryRepo.GetAll();
            Tags = _tagRepo.GetAll();

            return Partial("Modals/_CreateModal", this);
        }
        public IActionResult OnPostCreate()
        {
            if (string.IsNullOrWhiteSpace(CreateArticle.Headline))
            {
                ModelState.AddModelError(
                    "CreateArticle.Headline",
                    "Headline is required");
            }

            //if (!ModelState.IsValid)
            //{
            //    Categories = _categoryRepo.GetAll();
            //    Tags = _tagRepo.GetAll();
            //    return Partial("_CreateModal", this);
            //}
            CreateArticle.CreatedDate = DateTime.Now;

            _newsRepo.Add(CreateArticle);

            // Add Tags
            CreateTagIds ??= new List<int>();

            foreach (var tagId in CreateTagIds)
            {
                _newsRepo.AddTag(CreateArticle.NewsArticleId, tagId);
            }

            return new JsonResult(new { success = true });
        }
        public IActionResult OnGetDelete(string id)
        {
            var staffId = (short?)HttpContext.Session.GetInt32("AccountId");

            if (staffId is null)
                return RedirectToPage("/Auth/Login");

            var article = _newsRepo.GetById(id);

            if (article == null)
                return RedirectToPage();

            _newsRepo.Delete(id);

            return RedirectToPage();
        }
        public IActionResult OnGetDuplicate(string id)
        {
            var staffId = (short?)HttpContext.Session.GetInt32("AccountId");

            if (staffId is null)
                return RedirectToPage("/Auth/Login");

            var article = _newsRepo.GetById(id);

            if (article == null)
                return RedirectToPage();


            var duplicated = new NewsArticle
            {
                NewsTitle = article.NewsTitle + " (Copy)",
                Headline = article.Headline,
                NewsContent = article.NewsContent,
                NewsSource = article.NewsSource,
                CategoryId = article.CategoryId,
                CreatedById = staffId,
                CreatedDate = DateTime.Now,
                NewsStatus = false
            };

            _newsRepo.Add(duplicated);

            // Copy Tags
            if (article.Tags != null)
            {
                foreach (var tag in article.Tags)
                {
                    _newsRepo.AddTag(duplicated.NewsArticleId, tag.TagId);
                }
            }

            return RedirectToPage();
        }
    }
}