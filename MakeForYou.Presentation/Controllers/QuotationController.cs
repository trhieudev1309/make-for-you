using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
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

        public QuotationController(IQuotationService service)
        {
            _service = service;
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
                1 => "Accepted",
                2 => "Rejected",
                3 => "Confirmed",
                4 => "Cancelled",
                _ => "Unknown"
            }
        };

        // POST api/quotation
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuotationRequest req)
        {
            try
            {
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

        // POST api/quotation/{id}/accept  — Buyer
        [HttpPost("{id:long}/accept")]
        public async Task<IActionResult> Accept(long id)
        {
            try
            {
                await _service.AcceptAsync(id, CurrentUserId);
                var q = await _service.GetByIdAsync(id);
                return Ok(MapQuotation(q!));
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // POST api/quotation/{id}/reject  — Buyer
        [HttpPost("{id:long}/reject")]
        public async Task<IActionResult> Reject(long id)
        {
            try
            {
                await _service.RejectAsync(id, CurrentUserId);
                var q = await _service.GetByIdAsync(id);
                return Ok(MapQuotation(q!));
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // POST api/quotation/{id}/confirm  — Seller
        [HttpPost("{id:long}/confirm")]
        public async Task<IActionResult> Confirm(long id)
        {
            try
            {
                await _service.ConfirmAsync(id, CurrentUserId);
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