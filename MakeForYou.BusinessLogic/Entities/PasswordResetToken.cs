using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities
{
    public class PasswordResetToken
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;      // stored as SHA-256 hash

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
