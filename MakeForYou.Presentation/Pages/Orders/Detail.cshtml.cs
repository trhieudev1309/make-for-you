using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Enums;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Orders
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly IOrderService _orderService;
        public DetailModel(IOrderService orderService) => _orderService = orderService;

        public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var buyerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Order = await _orderService.GetOrderDetailAsync(id, buyerId);

            if (Order == null) return NotFound();
            return Page();
        }

        public static string BadgeClass(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending => "bg-secondary",
            OrderStatus.Quoted => "bg-info text-dark",
            OrderStatus.Confirmed => "bg-primary",
            OrderStatus.InProgress => "bg-warning text-dark",
            OrderStatus.Completed => "bg-success",
            OrderStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };

        public static string StatusIcon(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending => "bi-hourglass",
            OrderStatus.Quoted => "bi-tag",
            OrderStatus.Confirmed => "bi-check-circle",
            OrderStatus.InProgress => "bi-tools",
            OrderStatus.Completed => "bi-bag-check",
            OrderStatus.Cancelled => "bi-x-circle",
            _ => "bi-circle"
        };
    }
}