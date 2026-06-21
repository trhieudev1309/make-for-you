using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class RequestsModel : PageModel
    {
        private readonly IOrderService _orderService;
        public RequestsModel(IOrderService orderService) => _orderService = orderService;

        public List<Order> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var sellerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Orders = await _orderService.GetRequestsBySellerAsync(sellerId);
            return Page();
        }

        public static string StatusLabel(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending                => "Chờ thanh toán",
            OrderStatus.Confirmed              => "Đã thanh toán",
            OrderStatus.Quoted                 => "Đã báo giá",
            OrderStatus.InProgress             => "Đang thực hiện",
            OrderStatus.Completed              => "Hoàn thành sản phẩm",
            OrderStatus.Delivering             => "Đang giao hàng",
            OrderStatus.Delivered              => "Đã giao hàng",
            OrderStatus.Done                   => "Đã xong",
            OrderStatus.Cancelled              => "Đã hủy",
            OrderStatus.PendingQuotationSubmit  => "Chờ nghệ nhân báo giá",
            OrderStatus.PendingQuotationAccept  => "Chờ chấp nhận báo giá",
            OrderStatus.PendingQuotationPayment => "Chờ thanh toán báo giá",
            _                                  => "Không xác định"
        };
    }
}