using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class RegisterRequest
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        // 8–15 chars, mixed: validated in AuthService
        [Required, MinLength(8), MaxLength(15)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(50), RegularExpression(@"^[0-9]{9,11}$", ErrorMessage = "Phone must be 9–11 digits.")]
        public string? Phone { get; set; }

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Buyer = 0, Seller = 1  (Admin cannot self-register)
        [Required, Range(0, 1, ErrorMessage = "Role must be Buyer (0) or Seller (1).")]
        public int Role { get; set; }
    }
}
