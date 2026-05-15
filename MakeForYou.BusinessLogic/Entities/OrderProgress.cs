using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class OrderProgress
    {
        [Key]
        public long ProgressId { get; set; }

        public long OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;

        [MaxLength(500)]
        public string? Note { get; set; }           // seller's update message

        public string? ImageUrl { get; set; }       // uploaded progress photo

        public int StatusSnapshot { get; set; }     // order status at time of log

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}