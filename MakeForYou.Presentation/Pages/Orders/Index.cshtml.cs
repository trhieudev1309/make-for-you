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
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        public IndexModel(IOrderService orderService) => _orderService = orderService;

        public List<Order> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var buyerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Orders = await _orderService.GetOrdersByUserAsync(buyerId);
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
    }
}