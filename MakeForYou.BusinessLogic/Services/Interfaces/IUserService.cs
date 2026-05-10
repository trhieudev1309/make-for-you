using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        // Mình ĐÃ XÓA hàm UpdateRoleAsync cũ đi rồi nhé
        Task<User?> FindByIdAsync(long id);
        Task<List<User>> GetAllUsersAsync();

        // Hàm mới gộp cả 3 thông tin
        Task<bool> UpdateUserAsync(long id, string fullName, string phone, int newRole);

        Task<bool> UpdateStatusAsync(long userId, int status);
        Task<(List<User> Users, int TotalCount)> GetFilteredUsersAsync(string? search, int? role, int? status, int pageIndex, int pageSize);
        Task<bool> AddUserAsync(User user, string password);
    }
}