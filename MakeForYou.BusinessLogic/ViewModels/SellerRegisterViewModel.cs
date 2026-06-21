using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.ViewModels
{
    public class SellerRegisterViewModel
    {
        // ── Thông tin gian hàng ──────────────────────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập tên shop")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Tên shop từ 3–30 ký tự")]
        [Display(Name = "Tên Shop")]
        public string ShopName { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Mô tả tối đa 300 ký tự")]
        [Display(Name = "Mô tả Shop")]
        public string? ShopDescription { get; set; }

        // ── Thông tin liên hệ ────────────────────────────────────────────
        // Optional: the Register form no longer collects this separately — it reuses
        // PickupPhone as the seller's contact number (see SellerService.RegisterSellerAsync).
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        // ── Địa chỉ lấy hàng ─────────────────────────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [Display(Name = "Họ & Tên")]
        public string PickupFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại lấy hàng")]
        [RegularExpression(@"^0[35789]\d{8}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0912345678)")]
        [Display(Name = "Số điện thoại lấy hàng")]
        public string PickupPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn tỉnh / thành phố")]
        [Display(Name = "Tỉnh / Thành phố")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn quận / huyện")]
        [Display(Name = "Quận / Huyện")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phường / xã")]
        [Display(Name = "Phường / Xã")]
        public string Ward { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn tỉnh / thành phố hợp lệ")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn tỉnh / thành phố")]
        public int ProvinceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quận / huyện hợp lệ")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn quận / huyện")]
        public int DistrictId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phường / xã hợp lệ")]
        public string WardCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string AddressDetail { get; set; } = string.Empty;

        // ── Thông tin ngân hàng nhận tiền ─────────────────────────────────
        [Display(Name = "Ngân hàng")]
        public string? BankBin { get; set; }

        [Display(Name = "Số tài khoản")]
        public string? BankAccountNumber { get; set; }

        [Display(Name = "Tên chủ tài khoản")]
        public string? BankAccountName { get; set; }

        // ── Xác nhận điều khoản ──────────────────────────────────────────
        [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn cần đồng ý với điều khoản để tiếp tục")]
        [Display(Name = "Đồng ý điều khoản")]
        public bool AgreedToTerms { get; set; }
    }
}
