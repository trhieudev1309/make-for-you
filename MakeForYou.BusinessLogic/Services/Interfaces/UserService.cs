using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces; // Link đến IUserService
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.BusinessLogic.Services.Implementations // Sửa lại namespace cho đúng chuẩn
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByIdAsync(long id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        // --- Cập nhật trạng thái (Khóa/Mở) ---
        public async Task<bool> UpdateStatusAsync(long userId, int status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }

        // --- Lấy danh sách có Lọc và Phân trang (Rất tốt!) ---
        public async Task<(List<User> Users, int TotalCount)> GetFilteredUsersAsync(
            string? search, int? role, int? status, int pageIndex, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            if (role.HasValue) query = query.Where(u => u.Role == role.Value);
            if (status.HasValue) query = query.Where(u => u.Status == status.Value);

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        // --- Thêm User mới (Admin tạo) ---
        public async Task<bool> AddUserAsync(User user, string password)
        {
            // Note: Tiến nhớ gọi hàm Hash mật khẩu ở đây trước khi gán nhé!
            user.PasswordHash = password;
            user.CreatedAt = DateTime.UtcNow;
            user.Status = (int)UserStatus.Active;

            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- Cập nhật Full thông tin (Gộp chung cực tiện) ---
        public async Task<bool> UpdateUserAsync(long id, string fullName, string phone, int newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.FullName = fullName;
            user.Phone = phone;
            user.Role = newRole;

            // Dùng Update để tường minh hơn
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}