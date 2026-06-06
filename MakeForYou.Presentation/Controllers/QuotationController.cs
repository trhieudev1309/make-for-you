using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MakeForYou.Presentation.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/quotation")]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _service;
        private readonly IOrderService _orderService;

        public QuotationController(IQuotationService service, IOrderService orderService)
        {
            _service = service;
            _orderService = orderService;
        }

        private long CurrentUserId =>
            long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private object MapQuotation(Quotation q) => new
        {
            quotationId = q.QuotationId,
            orderId = q.OrderId,
            sellerId = q.CreatedBy,
            proposedPrice = q.ProposedPrice,
            message = q.Message,
            deadline = q.Deadline,
            createdAt = q.CreatedAt,
            statusLabel = q.Status switch
            {
                0 => "Pending",
                1 => "Approved",
                2 => "Cancelled",
                _ => "Unknown"
            }
        };

        // POST api/quotation
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuotationRequest req)
        {
            try
            {
                if (req.ProposedPrice < 5000)
                    return BadRequest(new { error = "Giá tối thiểu là 5,000 VNĐ." });

                var order = await _orderService.GetOrderForSellerAsync(req.OrderId, CurrentUserId);
                if (order == null)
                    return Forbid();

                if (order.Status != (int)OrderStatus.Confirmed)
                    return BadRequest(new { error = "Chỉ có thể tạo báo giá cho đơn hàng đã xác nhận." });

                var existing = await _service.GetByOrderAsync(req.OrderId);
                if (existing.Any(q => q.Status == 0))
                    return BadRequest(new { error = "Đã có một báo giá đang chờ xử lý cho đơn hàng này." });

                var quotation = new Quotation
                {
                    OrderId = req.OrderId,
                    CreatedBy = CurrentUserId,
                    ProposedPrice = req.ProposedPrice,
                    Message = req.Message,
                    Deadline = req.Deadline,
                    Status = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _service.CreateAsync(quotation);
                return Ok(MapQuotation(quotation));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        // GET api/quotation/order/{orderId}
        [HttpGet("order/{orderId:long}")]
        public async Task<IActionResult> GetByOrder(long orderId)
        {
            try
            {
                var list = await _service.GetByOrderAsync(orderId);
                return Ok(list.Select(MapQuotation));
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        // POST api/quotation/{id}/approve  — Buyer
        [HttpPost("{id:long}/approve")]
        public async Task<IActionResult> Approve(long id)
        {
            try
            {
                await _service.ApproveAsync(id, CurrentUserId);
                var q = await _service.GetByIdAsync(id);
                return Ok(MapQuotation(q!));
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // POST api/quotation/{id}/cancel  — Seller or Buyer
        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            try
            {
                await _service.CancelAsync(id, CurrentUserId);
                var q = await _service.GetByIdAsync(id);
                return Ok(MapQuotation(q!));
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }
    }

    public class CreateQuotationRequest
    {
        public long OrderId { get; set; }
        public int ProposedPrice { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
    }
}
