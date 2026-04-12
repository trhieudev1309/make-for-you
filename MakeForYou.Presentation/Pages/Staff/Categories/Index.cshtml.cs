using FUNews.BusinessLogic.Entities;
using FUNews.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNews.Presentation.Pages.Staff.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepo;

        public IndexModel(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public Dictionary<short, int> ArticleCounts { get; set; } = new();
        [BindProperty]
        public Category EditCategory { get; set; }
        [BindProperty]
        public Category CreateCategory { get; set; } = default!;

        public List<Category> AllCategories { get; set; } = new();

        public bool IsUsed { get; set; }

        public void OnGet()
        {
            Categories = string.IsNullOrWhiteSpace(Keyword)
                ? _categoryRepo.GetAll()
                : _categoryRepo.Search(Keyword);

            ArticleCounts = _categoryRepo.GetArticleCountByCategory();
        }

        public IActionResult OnGetDelete(short id)
        {
            if (_categoryRepo.IsUsedByArticles(id))
            {
                TempData["Error"] = "Cannot delete category used by articles.";
                return RedirectToPage();
            }

            _categoryRepo.Delete(id);
            return RedirectToPage();
        }

        public IActionResult OnGetToggle(short id)
        {
            var category = _categoryRepo.GetById(id);
            if (category == null)
                return RedirectToPage();

            if (category.IsActive)
                _categoryRepo.Deactivate(id);
            else
                _categoryRepo.Activate(id);

            return RedirectToPage();
        }

        public IActionResult OnGetCreate()
        {
            AllCategories = _categoryRepo.GetAll().ToList();

            CreateCategory = new Category
            {
                IsActive = true
            };

            return Partial("Modals/_CreateModal", this);
        }

        public IActionResult OnPostCreate()
        {
            AllCategories = _categoryRepo.GetAll().ToList();

            var allSubCats = AllCategories
                .Where(x => CreateCategory.ParentCategoryId != null &&
                            x.ParentCategoryId == CreateCategory.ParentCategoryId);

            if (allSubCats.Any(x =>
                x.CategoryName == CreateCategory.CategoryName.Trim()))
            {
                ModelState.AddModelError("CreateCategory.CategoryName",
                    "Category name under the same parent must be unique");
            }

            //if (!ModelState.IsValid)
            //{
            //    return Partial("Modals/_CreateFormBody", this);
            //}

            CreateCategory.IsActive = true;
            _categoryRepo.Add(CreateCategory);

            return new JsonResult(new { success = true });
        }

        public IActionResult OnGetEdit(short id)
        {
            EditCategory = _categoryRepo.GetById(id);
            AllCategories = _categoryRepo.GetAll().ToList();
            IsUsed = _categoryRepo.IsUsedByArticles(id);

            return Partial("Modals/_EditModal", this);
        }

        public IActionResult OnPostEdit()
        {
            //if (!ModelState.IsValid)
            //{
            //    AllCategories = _categoryRepo.GetAll().ToList();
            //    IsUsed = _categoryRepo.IsUsedByArticles(EditCategory.CategoryId);
            //    return Partial("Modals/_EditModal", this);
            //}

            _categoryRepo.Update(EditCategory);
            return new JsonResult(new { success = true });
        }
    }
}