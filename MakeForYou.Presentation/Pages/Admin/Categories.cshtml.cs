using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CategoriesModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public CategoriesModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public List<CategoryViewModel> CategoryList { get; set; } = new();

        [BindProperty]
        public CategoryRequest NewCategory { get; set; } = new();

        // Thuộc tính để nhận dữ liệu khi sửa từ Modal
        [BindProperty]
        public long EditCategoryId { get; set; }
        [BindProperty]
        public string EditCategoryName { get; set; }

        public async Task OnGetAsync()
        {
            CategoryList = await _categoryService.GetAllCategoriesAsync();
        }

        // Handler: Tạo mới
        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(NewCategory.Name))
            {
                await _categoryService.CreateCategoryAsync(NewCategory.Name);
            }
            return RedirectToPage();
        }

        // Handler: Cập nhật tên danh mục
        public async Task<IActionResult> OnPostEditAsync()
        {
            if (EditCategoryId > 0 && !string.IsNullOrEmpty(EditCategoryName))
            {
                await _categoryService.UpdateCategoryAsync(EditCategoryId, EditCategoryName);
            }
            return RedirectToPage();
        }

        // Handler: Xóa danh mục
        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            // Lưu ý: Service của Tiến nên check nếu ProductCount > 0 thì không cho xóa
            await _categoryService.DeleteCategoryAsync(id);
            return RedirectToPage();
        }
    }
}