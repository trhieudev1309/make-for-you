using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;

        public UsersModel(IUserService userService)
        {
            _userService = userService;
        }

        public List<User> UserList { get; set; } = new();

        // --- Các thuộc tính hỗ trợ Filter & Paging ---
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int? RoleFilter { get; set; }
        [BindProperty(SupportsGet = true)] public int? StatusFilter { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        // --- Thuộc tính để thêm User mới ---
        [BindProperty] public User NewUser { get; set; } = new();
        [BindProperty] public string NewUserPassword { get; set; } = string.Empty;

        // --- HANDLER: Lấy danh sách (Có tìm kiếm, lọc, phân trang) ---
        public async Task OnGetAsync()
        {
            var result = await _userService.GetFilteredUsersAsync(
                SearchTerm, RoleFilter, StatusFilter, CurrentPage, PageSize);

            UserList = result.Users;
            TotalPages = (int)Math.Ceiling(result.TotalCount / (double)PageSize);
        }

        // --- HANDLER: Thêm người dùng mới ---
        public async Task<IActionResult> OnPostAddUserAsync()
        {
            if (!string.IsNullOrEmpty(NewUser.Email) && !string.IsNullOrEmpty(NewUserPassword))
            {
                // NewUser lúc này đã tự có FullName, Email, Phone, Role nhờ BindProperty
                await _userService.AddUserAsync(NewUser, NewUserPassword);
            }
            return RedirectToPage();
        }

        // --- HANDLER: Cập nhật thông tin người dùng (Tên, SĐT, Vai trò) ---
        // Lưu ý: Tên handler OnPostUpdateUserAsync khớp với asp-page-handler="UpdateUser"
        public async Task<IActionResult> OnPostUpdateUserAsync(long id, string fullName, string phone, int newRole)
        {
            // Tiến hãy đảm bảo trong IUserService đã có hàm UpdateUserAsync nhận các tham số này
            await _userService.UpdateUserAsync(id, fullName, phone, newRole);
            return RedirectToPage();
        }

        // --- HANDLER: Khóa / Mở khóa tài khoản ---
        public async Task<IActionResult> OnPostUpdateStatusAsync(long id, int status)
        {
            await _userService.UpdateStatusAsync(id, status);
            return RedirectToPage();
        }
    }
}