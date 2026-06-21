using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Checkout
{
    public class ReturnModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderRepository _orderRepo;

        public ReturnModel(IPaymentService paymentService, IOrderRepository orderRepo)
        {
            _paymentService = paymentService;
            _orderRepo = orderRepo;
        }

        public bool IsSuccess { get; private set; }
        public long OrderCode { get; private set; }
        public string StatusMessage { get; private set; } = string.Empty;
        /// <summary>OrderId of the matching order — used for the direct "Xem đơn hàng" button.</summary>
        public long? RelatedOrderId { get; private set; }

        public async Task<IActionResult> OnGetAsync(long orderCode)
        {
            if (orderCode <= 0) return RedirectToPage("/Index");

            OrderCode = orderCode;

            try
            {
                var status = await _paymentService.GetPaymentStatusAsync(orderCode);
                // PayOS SDK returns the status enum's .ToString() (e.g. "Paid"), which does
                // not match the raw API string "PAID" with an ordinal comparison — that
                // mismatch made every successful payment look unpaid on this page.
                IsSuccess = string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase);
                StatusMessage = IsSuccess
                    ? "Thanh toán thành công!"
                    : "Thanh toán chưa hoàn tất.";
            }
            catch
            {
                IsSuccess = false;
                StatusMessage = "Không thể xác nhận trạng thái thanh toán. Vui lòng kiểm tra trang đơn hàng.";
            }

            // Read-only: resolve the orderId so the view can link directly to the order.
            // Order status is updated exclusively by the PayOS webhook (POST /api/payment/webhook).
            try
            {
                var orders = await _orderRepo.FindByPaymentCodeAsync(orderCode);
                RelatedOrderId = orders.FirstOrDefault()?.OrderId;
            }
            catch { /* non-critical — link falls back to /Orders/Index */ }

            return Page();
        }
    }
}
