using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;
    }
}
