using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities.DTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static AuthResult Ok(string msg) => new() { Success = true, Message = msg };
        public static AuthResult Fail(string msg) => new() { Success = false, Message = msg };
    }
}
