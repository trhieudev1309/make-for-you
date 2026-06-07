using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class CustomizationGroup
    {
        [Key]
        public long CustomizationGroupId { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Title { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Status { get; set; } // 0 = Inactive, 1 = Active

        // Navigation
        public ICollection<CustomizationOption> Options { get; set; } = new List<CustomizationOption>();
    }
}