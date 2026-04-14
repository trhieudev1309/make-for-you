using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.Repositories.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> FindByEmailAsync(string email) =>
    await _context.Users
             .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

        public async Task SavePasswordResetTokenAsync(PasswordResetToken token)
        {
            // Kiểm tra xem token này đã có trong database chưa
            // Nếu token.Id > 0 và tồn tại trong DB thì là Update, ngược lại là Add
            var exists = await _context.PasswordResetTokens.AnyAsync(t => t.Id == token.Id);

            if (exists)
            {
                _context.PasswordResetTokens.Update(token);
            }
            else
            {
                _context.PasswordResetTokens.Add(token);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> FindValidResetTokenAsync(string email, string tokenHash) =>
    await _context.PasswordResetTokens
             .Include(t => t.User)
             .FirstOrDefaultAsync(t =>
                 t.Token == tokenHash &&
                 t.User.Email == email.ToLower().Trim() &&
                 !t.IsUsed &&
                 t.ExpiresAt > DateTime.UtcNow);

        public async Task InvalidateUserTokensAsync(long userId)
        {
            var tokens = await _context.PasswordResetTokens
                                  .Where(t => t.UserId == userId && !t.IsUsed)
                                  .ToListAsync();
            tokens.ForEach(t => t.IsUsed = true);
            await _context.SaveChangesAsync();
        }
    }
}
