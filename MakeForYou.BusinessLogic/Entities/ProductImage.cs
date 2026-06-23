using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class ProductImage
    {
        [Key]
        public long ProductImageId { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Url { get; set; } = string.Empty;

        // Display order, lower values appear first
        public int DisplayOrder { get; set; } = 0;

        // Optional flag for primary image
        public bool IsPrimary { get; set; } = false;
    }
}
