using MakeForYou.BusinessLogic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<MakeForYou.BusinessLogic.Entities.User> CreateUserAsync(MakeForYou.BusinessLogic.Entities.User user);
        Task<User?> FindByEmailAsync(string email);
        Task SavePasswordResetTokenAsync(PasswordResetToken token);
        Task<PasswordResetToken?> FindValidResetTokenAsync(string email, string tokenHash);
        Task InvalidateUserTokensAsync(long userId);
        Task<User?> FindByIdAsync(long id);
    }
}
