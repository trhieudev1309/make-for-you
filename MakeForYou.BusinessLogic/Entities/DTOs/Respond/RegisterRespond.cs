using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Respond
{
    public class RegisterRespond
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long? UserId { get; set; }
    }
}
