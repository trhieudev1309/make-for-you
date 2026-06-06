using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Services;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Checkout
{
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IGhnService _ghnService;
        private readonly IProductRepository _productRepo;

        public IndexModel(
            ICartService cartService, 
            IOrderService orderService, 
            IPaymentService paymentService,
            IGhnService ghnService,
            IProductRepository productRepo)
        {
            _cartService = cartService;
            _orderService = orderService;
            _paymentService = paymentService;
            _ghnService = ghnService;
            _productRepo = productRepo;
        }

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public int TotalAmount { get; set; }

        [BindProperty]
        public CheckoutRequest OrderRequest { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");

            long userId = long.Parse(userIdStr);
            CartItems = await _cartService.GetCartAsync(userId);
            if (!CartItems.Any()) return RedirectToPage("/Cart/Index");

            TotalAmount = CartItems.Sum(x => x.TotalPrice);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");
            long userId = long.Parse(userIdStr);

            var cartItems = await _cartService.GetCartAsync(userId);
            if (!cartItems.Any()) return RedirectToPage("/Cart/Index");

            var paymentCode = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L);

            // THAY ĐỔI TẠI ĐÂY: Truyền trọn gói đối tượng OrderRequest đã chứa đủ cấu trúc địa chỉ
            var orders = await _orderService.CreateOrderFromCartAsync(userId, OrderRequest, paymentCode);

            if (orders == null || !orders.Any()) return Page();

            var totalAmount = orders.Sum(o => (o.AgreedPrice ?? 0) + (o.ShippingFee ?? 0));
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var checkoutUrl = await _paymentService.CreatePaymentLinkAsync(paymentCode, totalAmount, baseUrl, cartItems);

            return Redirect(checkoutUrl);
        }

        public async Task<IActionResult> OnGetCalculateShippingFeeAsync(int districtId, string wardCode)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return new JsonResult(new { success = false, message = "Unauthorized" });

            long userId = long.Parse(userIdStr);
            var cartItems = await _cartService.GetCartAsync(userId);
            if (!cartItems.Any()) return new JsonResult(new { success = false, message = "Cart is empty" });

            var products = new List<Product>();
            foreach (var item in cartItems)
            {
                var product = await _productRepo.FindByIdAsync(item.ProductId);
                if (product != null)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        products.Add(product);
                    }
                }
            }

            try
            {
                int fee = await _ghnService.CalculateShippingFeeAsync(products, districtId, wardCode);
                return new JsonResult(new { success = true, fee = fee });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
