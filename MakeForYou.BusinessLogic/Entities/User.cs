using System.ComponentModel.DataAnnotations;

namespace MakeForYou.BusinessLogic.Entities
{
    public class User
    {
        [Key]
        public long UserId { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [MaxLength(50)]
        public string? Phone { get; set; }

        public int Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Status { get; set; }

        // Navigation
        public Buyer? Buyer { get; set; }
        public Seller? Seller { get; set; }

        // Orders where this user is the buyer
        public ICollection<Order>? OrdersAsBuyer { get; set; }

        public ICollection<Review>? Reviews { get; set; }
        public ICollection<ChatMessage>? SentMessages { get; set; }
        public ICollection<Quotation>? CreatedQuotations { get; set; }
        public ICollection<Notification>? Notifications { get; set; }

    }
}