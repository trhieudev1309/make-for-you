using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Checkout
{
    public class ReturnModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public ReturnModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public bool IsSuccess { get; private set; }
        public long OrderCode { get; private set; }
        public string StatusMessage { get; private set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(long orderCode)
        {
            if (orderCode <= 0) return RedirectToPage("/Index");

            OrderCode = orderCode;

            try
            {
                var status = await _paymentService.GetPaymentStatusAsync(orderCode);
                IsSuccess = status == "Paid";
                StatusMessage = IsSuccess
                    ? "Thanh toán thành công!"
                    : "Thanh toán chưa hoàn tất.";
            }
            catch
            {
                IsSuccess = false;
                StatusMessage = "Không thể xác nhận trạng thái thanh toán. Vui lòng kiểm tra đơn hàng của bạn.";
            }

            return Page();
        }
    }
}
