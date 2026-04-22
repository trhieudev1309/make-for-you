namespace MakeForYou.BusinessLogic.Entities.DTOs.Respond
{
    public class CartItemViewModel
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ImageUrl { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }

        // Thành tiền cho từng món
        public int TotalPrice => Price * Quantity;
    }
}