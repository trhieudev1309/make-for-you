using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterRespond> RegisterAsync(RegisterRequest req);
    }
}
