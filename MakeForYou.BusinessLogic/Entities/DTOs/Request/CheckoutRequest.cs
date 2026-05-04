namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CheckoutRequest
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string ShippingAddress { get; set; } = null!;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "Online"; // Mặc định online
    }
}