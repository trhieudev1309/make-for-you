using System.ComponentModel.DataAnnotations;

namespace MakeForYou.BusinessLogic.Entities
{
    public class Category
    {
        [Key]
        public long CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}