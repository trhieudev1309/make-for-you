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
    public class RequestDetailModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IQuotationService _quotationService;

        public RequestDetailModel(IOrderService orderService, IQuotationService quotationService)
        {
            _orderService = orderService;
            _quotationService = quotationService;
        }

        public Order? Order { get; set; }
        public long SellerId { get; set; }

        private long GetSellerId() =>
            long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> OnGetAsync(long id)
        {
            SellerId = GetSellerId();
            Order = await _orderService.GetRequestDetailAsync(id, SellerId);
            if (Order == null) return NotFound();
            return Page();
        }

        // ── Send Quotation ──────────────────────────────────────────────────
        public async Task<IActionResult> OnPostSendQuoteAsync(
            long id, decimal ProposedPrice, string? Message)
        {
            SellerId = GetSellerId();
            Order = await _orderService.GetRequestDetailAsync(id, SellerId);
            if (Order == null) return NotFound();

            await _quotationService.CreateAsync(new Quotation
            {
                OrderId = Order.OrderId,
                ProposedPrice = (int)ProposedPrice,
                Message = Message,
                Status = 0, // Pending buyer acceptance
                CreatedAt = DateTime.UtcNow
            });

            // Bump order status to Quoted if still Pending
            if (Order.Status == (int)OrderStatus.Pending)
                await _orderService.UpdateStatusAsync(Order.OrderId, (int)OrderStatus.Quoted);

            TempData["Success"] = "Quotation sent to buyer.";
            return RedirectToPage(new { id });
        }

        // ── Update Order Status ────────────────────────────────────────────
        public async Task<IActionResult> OnPostUpdateStatusAsync(long id, int newStatus)
        {
            SellerId = GetSellerId();
            Order = await _orderService.GetRequestDetailAsync(id, SellerId);
            if (Order == null) return NotFound();

            await _orderService.UpdateStatusAsync(Order.OrderId, newStatus);

            TempData["Success"] = $"Order status updated to {(OrderStatus)newStatus}.";
            return RedirectToPage(new { id });
        }
    }
}