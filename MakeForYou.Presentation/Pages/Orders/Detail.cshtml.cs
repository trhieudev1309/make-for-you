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

        // Matches the custom CSS badge classes in Detail.cshtml
        public static string BadgeClass(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending => "badge-secondary",
            OrderStatus.Quoted => "badge-info",
            OrderStatus.Confirmed => "badge-primary",
            OrderStatus.InProgress => "badge-warning",
            OrderStatus.Completed => "badge-success",
            OrderStatus.Cancelled => "badge-danger",
            _ => "badge-secondary"
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