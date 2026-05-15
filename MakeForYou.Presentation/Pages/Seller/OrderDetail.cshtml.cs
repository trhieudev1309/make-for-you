using MakeForYou.BusinessLogic.DTOs;          // ← UpdateProgressRequest lives here
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class OrderDetailModel : PageModel
    {
        private readonly IOrderService _orderService;
        public OrderDetailModel(IOrderService orderService) => _orderService = orderService;

        public Order? Order { get; set; }

        [BindProperty]
        public UpdateProgressRequest ProgressInput { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        private long GetSellerId() =>
            long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> OnGetAsync(long id)
        {
            Order = await _orderService.GetOrderForSellerAsync(id, GetSellerId());
            if (Order == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long id)
        {
            Order = await _orderService.GetOrderForSellerAsync(id, GetSellerId());
            if (Order == null) return NotFound();

            if (!ModelState.IsValid)
                return Page();

            var result = await _orderService.UpdateProgressAsync(id, GetSellerId(), ProgressInput);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            SuccessMessage = result.Message;
            Order = await _orderService.GetOrderForSellerAsync(id, GetSellerId());
            return Page();
        }

        public List<(int Value, string Label)> AllowedStatuses()
        {
            if (Order == null) return new();
            return Enum.GetValues<OrderStatus>()
                       .Where(s => (int)s > Order.Status && s != OrderStatus.Cancelled)
                       .Select(s => ((int)s, s.ToString()))
                       .ToList();
        }

        // Non-static so Razor can call via Model.BadgeClass(...)
        public string BadgeClass(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending => "badge-secondary",
            OrderStatus.Quoted => "badge-info",
            OrderStatus.Confirmed => "badge-primary",
            OrderStatus.InProgress => "badge-warning",
            OrderStatus.Completed => "badge-success",
            OrderStatus.Cancelled => "badge-danger",
            _ => "badge-secondary"
        };

        public string StatusIcon(int status) => (OrderStatus)status switch
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