using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        // ── Send Quotation (old commission flow: Pending/Quoted orders) ────────
        public async Task<IActionResult> OnPostSendQuoteAsync(
            long id, decimal ProposedPrice, string? Message, DateTime? Deadline)
        {
            SellerId = GetSellerId();
            Order = await _orderService.GetRequestDetailAsync(id, SellerId);
            if (Order == null) return NotFound();

            await _quotationService.CreateAsync(new Quotation
            {
                OrderId = Order.OrderId,
                CreatedBy = SellerId,
                ProposedPrice = (int)ProposedPrice,
                Message = Message,
                Deadline = Deadline.HasValue ? DateTime.SpecifyKind(Deadline.Value, DateTimeKind.Local).ToUniversalTime() : null,
                Status = 0,
                CreatedAt = DateTime.UtcNow
            });

            if (Order.Status == (int)OrderStatus.Pending)
                await _orderService.UpdateStatusAsync(Order.OrderId, (int)OrderStatus.Quoted);
            else if (Order.Status == (int)OrderStatus.PendingQuotationSubmit)
                await _orderService.UpdateStatusAsync(Order.OrderId, (int)OrderStatus.PendingQuotationAccept);

            TempData["Success"] = "Quotation sent to buyer.";
            return RedirectToPage(new { id });
        }

        // ── Send Add-on Quotation (cart orders: Confirmed with pending customizations) ──
        public async Task<IActionResult> OnPostSendAddonQuoteAsync(
            long id, int ProposedPrice, string? Message, DateTime? Deadline)
        {
            SellerId = GetSellerId();
            Order = await _orderService.GetRequestDetailAsync(id, SellerId);
            if (Order == null) return NotFound();

            if (Order.Status != (int)OrderStatus.Confirmed)
            {
                TempData["Error"] = "Báo giá bổ sung chỉ được tạo trên đơn hàng đã xác nhận.";
                return RedirectToPage(new { id });
            }

            if (ProposedPrice < 5000)
            {
                TempData["Error"] = "Giá bổ sung tối thiểu là 5,000 VNĐ.";
                return RedirectToPage(new { id });
            }

            var existing = await _quotationService.GetByOrderAsync(Order.OrderId);
            if (existing.Any(q => q.Status == 0))
            {
                TempData["Error"] = "Đã có một báo giá đang chờ xử lý.";
                return RedirectToPage(new { id });
            }

            await _quotationService.CreateAsync(new Quotation
            {
                OrderId = Order.OrderId,
                CreatedBy = SellerId,
                ProposedPrice = ProposedPrice,
                Message = Message,
                Deadline = Deadline.HasValue ? DateTime.SpecifyKind(Deadline.Value, DateTimeKind.Local).ToUniversalTime() : null,
                Status = 0,
                CreatedAt = DateTime.UtcNow
            });

            TempData["Success"] = "Đã gửi báo giá bổ sung đến người mua.";
            return RedirectToPage(new { id });
        }

        // ── Drop customization on a single item ────────────────────────────────
        public async Task<IActionResult> OnPostDropCustomizationAsync(long id, long itemId)
        {
            SellerId = GetSellerId();
            Order = await _orderService.GetRequestDetailAsync(id, SellerId);
            if (Order == null) return NotFound();

            await _orderService.DropCustomizationAsync(itemId);
            TempData["Success"] = "Yêu cầu tùy chỉnh đã được bỏ qua.";
            return RedirectToPage(new { id });
        }

        // ── Update Order Status ────────────────────────────────────────────────
        public async Task<IActionResult> OnPostUpdateStatusAsync(long id, int newStatus)
        {
            SellerId = GetSellerId();
            Order = await _orderService.GetRequestDetailAsync(id, SellerId);
            if (Order == null) return NotFound();

            if (newStatus == (int)OrderStatus.InProgress)
            {
                if (Order.Status != (int)OrderStatus.Confirmed)
                {
                    TempData["Error"] = "Chỉ có thể bắt đầu đơn hàng ở trạng thái Đã Xác Nhận.";
                    return RedirectToPage(new { id });
                }

                var hasUnresolved = Order.OrderItems?
                    .Any(i => i.HasCustomization && !i.IsCustomizationResolved) ?? false;
                if (hasUnresolved)
                {
                    TempData["Error"] = "Không thể bắt đầu khi vẫn còn yêu cầu tùy chỉnh chưa được xử lý.";
                    return RedirectToPage(new { id });
                }
            }

            if (newStatus == (int)OrderStatus.Cancelled)
            {
                var cancellable = new[] {
                    (int)OrderStatus.Pending, (int)OrderStatus.Quoted, (int)OrderStatus.Confirmed,
                    (int)OrderStatus.PendingQuotationSubmit, (int)OrderStatus.PendingQuotationAccept,
                    (int)OrderStatus.PendingQuotationPayment
                };
                if (!cancellable.Contains(Order.Status))
                {
                    TempData["Error"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
                    return RedirectToPage(new { id });
                }
            }

            await _orderService.UpdateStatusAsync(Order.OrderId, newStatus);
            TempData["Success"] = "Đã cập nhật trạng thái đơn hàng.";
            return RedirectToPage(new { id });
        }
    }
}
