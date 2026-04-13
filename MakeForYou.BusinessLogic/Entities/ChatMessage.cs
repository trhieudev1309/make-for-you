using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class ChatMessage
    {
        [Key]
        public long MessageId { get; set; }

        public long OrderId { get; set; }

        public long SenderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;

        [ForeignKey(nameof(SenderId))]
        public User Sender { get; set; } = null!;

        public string? MessageContent { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}