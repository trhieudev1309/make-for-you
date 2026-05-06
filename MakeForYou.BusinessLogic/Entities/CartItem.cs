using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class CartItem
    {
        [Key]
        public long CartItemId { get; set; }

        public long? UserId { get; set; } // Nullable nếu cho phép lưu nháp trước khi login

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        public int PriceAtAdd { get; set; } // Lưu giá tại thời điểm thêm để tránh biến động giá sau này
    }
}