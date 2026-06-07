using System.ComponentModel.DataAnnotations;

namespace MakeForYou.BusinessLogic.Entities
{
    public class OrderItem
    {
        public long OrderItemId { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; } // Giá lúc mua

        [MaxLength(2000)]
        public string? CustomizationsJson { get; set; }

        public bool HasCustomization { get; set; } = false;

        [MaxLength(100)]
        public string? CustomizationNote { get; set; }

        public bool IsCustomizationResolved { get; set; } = false;

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}