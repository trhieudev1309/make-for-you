using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Staff.Tags
{
    public class IndexModel : PageModel
    {
        private readonly ITagRepository _tagRepo;
        private readonly INewsArticleRepository _newsRepo;

        public IndexModel(
            ITagRepository tagRepo,
            INewsArticleRepository newsRepo)
        {
            _tagRepo = tagRepo;
            _newsRepo = newsRepo;
        }

        // ================== LIST ==================
        public IEnumerable<Tag> Tags { get; set; }
            = new List<Tag>();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public void OnGet()
        {
            var tags = _tagRepo.GetAll();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                tags = tags.Where(t =>
                    (t.TagName != null &&
                     t.TagName.Contains(Keyword))
                    ||
                    (t.Note != null &&
                     t.Note.Contains(Keyword)));
            }

            Tags = tags.OrderBy(t => t.TagName);
        }

        // ================== CREATE GET ==================
        [BindProperty]
        public Tag CreateTag { get; set; } = new();

        public IActionResult OnGetCreate()
        {
            CreateTag = new Tag();
            return Partial("Modals/_CreateModal", this);
        }

        // ================== CREATE POST ==================
        public IActionResult OnPostCreate()
        {
            if (string.IsNullOrWhiteSpace(CreateTag.TagName))
            {
                ModelState.AddModelError(
                    "CreateTag.TagName",
                    "Tag name is required");
            }

            if (_tagRepo.GetAll()
                .Any(x => x.TagName == CreateTag.TagName?.Trim()))
            {
                ModelState.AddModelError(
                    "CreateTag.TagName",
                    "Tag name can not be duplicated");
            }

            if (!ModelState.IsValid)
            {
                return Partial("Modals/_CreateFormBody", this);
            }

            _tagRepo.Add(CreateTag);

            return new JsonResult(new { success = true });
        }

        // ================== EDIT GET ==================
        [BindProperty]
        public Tag EditTag { get; set; }

        public IActionResult OnGetEdit(short id)
        {
            var tag = _tagRepo.GetById(id);

            if (tag == null)
                return NotFound();

            EditTag = tag;

            return Partial("Modals/_EditModal", this);
        }

        // ================== EDIT POST ==================
        public IActionResult OnPostEdit()
        {
            if (string.IsNullOrWhiteSpace(EditTag.TagName))
            {
                ModelState.AddModelError(
                    "EditTag.TagName",
                    "Tag name is required");
            }

            if (!ModelState.IsValid)
            {
                return Partial("Modals/_EditModal", this);
            }

            _tagRepo.Update(EditTag);

            return new JsonResult(new { success = true });
        }

        // ================== DELETE ==================
        public IActionResult OnGetDelete(short id)
        {
            var tag = _tagRepo.GetById(id);

            if (tag == null)
                return RedirectToPage();

            // Cannot delete if used by any article
            var usedArticles = _newsRepo
                .GetAll()
                .Any(a => a.Tags.Any(t => t.TagId == id));

            if (usedArticles)
            {
                TempData["Error"] =
                    "This tag is being used by articles and cannot be deleted.";
                return RedirectToPage();
            }

            _tagRepo.Delete(id);

            return RedirectToPage();
        }

        // ================== VIEW ARTICLES BY TAG ==================
        public IActionResult OnGetArticles(short id)
        {
            var tag = _tagRepo.GetById(id);

            if (tag == null)
                return RedirectToPage();

            var articles = _newsRepo
                .GetAll()
                .Where(a => a.Tags.Any(t => t.TagId == id))
                .ToList();

            ViewData["TagName"] = tag.TagName;
            ViewData["Articles"] = articles;

            return Page();
        }
    }
}