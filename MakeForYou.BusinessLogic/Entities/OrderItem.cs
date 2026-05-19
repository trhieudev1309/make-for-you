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

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}