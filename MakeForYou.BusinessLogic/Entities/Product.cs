using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class Product
    {
        [Key]
        public long ProductId { get; set; }

        [Required]
        public long SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        public Seller Seller { get; set; } = null!;

        // optional category reference (one product -> one category)
        public long? CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [MaxLength(250)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int? Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}