using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class SellerPost
    {
        [Key]
        public long PostId { get; set; }

        [Required]
        public long SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        public Seller Seller { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
