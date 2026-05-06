using MakeForYou.BusinessLogic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> FindByIdAsync(long id);
    }
}
