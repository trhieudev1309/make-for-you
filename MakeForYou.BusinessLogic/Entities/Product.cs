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

        // New: support multiple images per product
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public int? Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Status { get; set; }

        // Navigation - Add this line
        public ICollection<CustomizationGroup> CustomizationGroups { get; set; } = new List<CustomizationGroup>();

        public int Weight { get; set; } = 200; // Mặc định 200g nếu chưa nhập
        public int Length { get; set; } = 10;  // 10cm
        public int Width { get; set; } = 10;
        public int Height { get; set; } = 5;
    }
}