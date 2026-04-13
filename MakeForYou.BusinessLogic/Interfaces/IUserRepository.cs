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
    }
}
