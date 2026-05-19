using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class Order
    {
        [Key]
        public long OrderId { get; set; }

        [Required]
        public long BuyerId { get; set; }

        [Required]
        public long SellerId { get; set; }

        [ForeignKey(nameof(BuyerId))]
        public User Buyer { get; set; } = null!;

        [ForeignKey(nameof(SellerId))]
        public Seller Seller { get; set; } = null!;

        public string? OrderDescription { get; set; }

        public int? AgreedPrice { get; set; }

        public int Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public ICollection<Review>? Reviews { get; set; }
        public ICollection<ChatMessage>? ChatMessages { get; set; }
        public ICollection<Quotation>? Quotations { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Notification>? Notifications { get; set; }

        public ICollection<OrderProgress>? ProgressLogs { get; set; }
    }
}