namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CheckoutRequest
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string ShippingAddress { get; set; } = null!;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "Online"; // Mặc định online

        public int ShippingProvinceId { get; set; }
        public string ShippingProvinceName { get; set; } = null!;

        public int ShippingDistrictId { get; set; }
        public string ShippingDistrictName { get; set; } = null!;

        public string ShippingWardCode { get; set; } = null!;
        public string ShippingWardName { get; set; } = null!;

        public string ShippingAddressDetail { get; set; } = null!;
        public string PaymentMethod { get; set; } = "Online";
        public List<CartItemCustomization> Customizations { get; set; } = new();
    }
}