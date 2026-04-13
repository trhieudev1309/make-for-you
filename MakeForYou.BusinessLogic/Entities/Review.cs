using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class Review
    {
        [Key]
        public long ReviewId { get; set; }

        public long OrderId { get; set; }

        public long BuyerId { get; set; }

        public long SellerId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;

        [ForeignKey(nameof(BuyerId))]
        public User Buyer { get; set; } = null!;

        [ForeignKey(nameof(SellerId))]
        public Seller Seller { get; set; } = null!;

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}