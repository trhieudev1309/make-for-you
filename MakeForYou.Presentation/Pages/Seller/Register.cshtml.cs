using System.Security.Claims;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Seller
{
    public class RegisterModel : PageModel
    {
        private readonly ISellerService _sellerService;

        public RegisterModel(ISellerService sellerService)
        {
            _sellerService = sellerService;
        }

        public bool IsAlreadyRegistered { get; set; }

        public string ShopName { get; set; } = string.Empty;
        public string? ShopDescription { get; set; }
        public string PickupFullName { get; set; } = string.Empty;
        public string PickupPhone { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public string WardCode { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;
        public bool AgreedToTerms { get; set; }

        public string? BankBin { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }

        public async Task OnGetAsync()
        {
            var sellerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(sellerIdStr, out var sellerId)) return;

            var seller = await _sellerService.GetProfileAsync(sellerId);
            if (seller == null) return;

            IsAlreadyRegistered = seller.ShopName != null;

            ShopName = seller.ShopName ?? string.Empty;
            ShopDescription = seller.ShopDescription;
            PickupFullName = seller.PickupFullName ?? string.Empty;
            PickupPhone = seller.PickupPhone ?? string.Empty;
            Province = seller.Province ?? string.Empty;
            District = seller.District ?? string.Empty;
            Ward = seller.Ward ?? string.Empty;
            AddressDetail = seller.AddressDetail ?? string.Empty;
            AgreedToTerms = IsAlreadyRegistered;

            BankBin = seller.BankBin;
            BankAccountNumber = seller.BankAccountNumber;
            BankAccountName = seller.BankAccountName;
        }
    }
}
