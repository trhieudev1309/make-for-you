using MakeForYou.BusinessLogic.Entities.DTOs;
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
        Task<LoginResponse> LoginAsync(LoginRequest req);
        Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest req);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest req);
    }
}
