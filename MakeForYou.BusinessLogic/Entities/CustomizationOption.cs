using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class CustomizationOption
    {
        [Key]
        public long CustomizationOptionId { get; set; }

        [Required]
        public long CustomizationGroupId { get; set; }

        [ForeignKey(nameof(CustomizationGroupId))]
        public CustomizationGroup CustomizationGroup { get; set; } = null!;

        [Required, MaxLength(100)]
        public string OptionValue { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public int Status { get; set; } // 0 = Inactive, 1 = Active
    }
}