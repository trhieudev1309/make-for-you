using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Orders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        public IndexModel(IOrderService orderService) => _orderService = orderService;

        public List<Order> Orders { get; set; } = new();
        public bool IsSeller { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            IsSeller = User.IsInRole("Seller");

            if (IsSeller)
                Orders = await _orderService.GetRequestsBySellerAsync(userId);
            else
                Orders = await _orderService.GetOrdersByUserAsync(userId);

            return Page();
        }

        public static string BadgeClass(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending                => "bg-secondary",
            OrderStatus.Confirmed              => "bg-primary",
            OrderStatus.Quoted                 => "bg-info text-dark",
            OrderStatus.InProgress             => "bg-warning text-dark",
            OrderStatus.Completed              => "bg-success",
            OrderStatus.Delivering             => "badge-delivering",
            OrderStatus.Delivered              => "badge-delivered",
            OrderStatus.Done                   => "bg-success",
            OrderStatus.Cancelled              => "bg-danger",
            OrderStatus.PendingQuotationSubmit  => "badge-pqs",
            OrderStatus.PendingQuotationAccept  => "badge-pqa",
            OrderStatus.PendingQuotationPayment => "badge-pqp",
            _                                  => "bg-secondary"
        };

        public static string StatusIcon(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending                => "bi-hourglass",
            OrderStatus.Confirmed              => "bi-check-circle",
            OrderStatus.Quoted                 => "bi-tag",
            OrderStatus.InProgress             => "bi-tools",
            OrderStatus.Completed              => "bi-bag-check",
            OrderStatus.Delivering             => "bi-truck",
            OrderStatus.Delivered              => "bi-house-check",
            OrderStatus.Done                   => "bi-star-fill",
            OrderStatus.Cancelled              => "bi-x-circle",
            OrderStatus.PendingQuotationSubmit  => "bi-pencil-square",
            OrderStatus.PendingQuotationAccept  => "bi-tag",
            OrderStatus.PendingQuotationPayment => "bi-credit-card",
            _                                  => "bi-circle"
        };

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