using System.ComponentModel.DataAnnotations;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CheckoutRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận")]
        public string PhoneNumber { get; set; } = null!;

        public string? Note { get; set; }

        public string PaymentMethod { get; set; } = "Online";

        [Required]
        public int ShippingProvinceId { get; set; }

        [Required]
        public string ShippingProvinceName { get; set; } = null!;

        [Required]
        public int ShippingDistrictId { get; set; }

        [Required]
        public string ShippingDistrictName { get; set; } = null!;

        [Required]
        public string ShippingWardCode { get; set; } = null!;

        [Required]
        public string ShippingWardName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số nhà, tên đường cụ thể")]
        public string ShippingAddressDetail { get; set; } = null!;
        public List<CartItemCustomization> Customizations { get; set; } = new();
    }
}