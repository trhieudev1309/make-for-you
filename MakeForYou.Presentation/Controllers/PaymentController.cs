using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace MakeForYou.Presentation.Controllers
{
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderRepository _orderRepo;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            IOrderRepository orderRepo,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _orderRepo = orderRepo;
            _logger = logger;
        }

        /// <summary>
        /// PayOS calls this endpoint when a payment status changes.
        /// Always returns 200 so PayOS does not mark the URL as broken.
        /// Security: signature is verified via HMAC-SHA256 before any DB write.
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            Webhook? webhookBody = null;

            try
            {
                webhookBody = await System.Text.Json.JsonSerializer.DeserializeAsync<Webhook>(
                    Request.Body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PayOS webhook: failed to parse request body.");
                return Ok(new { success = true }); // still 200
            }

            if (webhookBody == null)
                return Ok(new { success = true });

            try
            {
                // Throws WebhookException if HMAC-SHA256 signature is invalid
                var data = await _paymentService.VerifyWebhookAsync(webhookBody);

                if (webhookBody.Code == "00")
                {
                    var orders = await _orderRepo.FindByPaymentCodeAsync(data.OrderCode);
                    if (orders.Any())
                    {
                        int expectedAmount;

                        var quotationOrder = orders.FirstOrDefault(
                            o => o.Status == (int)OrderStatus.PendingQuotationPayment);

                        if (quotationOrder != null)
                        {
                            // Quotation payment: validate against the accepted quotation's
                            // ProposedPrice. AgreedPrice must NOT be used — it already
                            // includes amounts paid in earlier rounds (e.g. initial cart
                            // checkout), so it would always look like an underpayment.
                            var acceptedQ = quotationOrder.Quotations?
                                .Where(q => q.Status == 1)
                                .OrderByDescending(q => q.CreatedAt)
                                .FirstOrDefault();
                            expectedAmount = acceptedQ?.ProposedPrice ?? 0;
                        }
                        else
                        {
                            // Initial checkout: validate against cart total PLUS shipping fee
                            // (the checkout payment link amount = AgreedPrice + ShippingFee).
                            expectedAmount = orders.Sum(o => (o.AgreedPrice ?? 0) + (o.ShippingFee ?? 0));
                        }

                        if (data.Amount < expectedAmount)
                        {
                            _logger.LogWarning(
                                "PayOS amount mismatch for orderCode {Code}: expected {Expected}, got {Actual}",
                                data.OrderCode, expectedAmount, data.Amount);
                        }
                        else
                        {
                            await _orderRepo.UpdatePaymentStatusByCodeAsync(data.OrderCode, true);
                            _logger.LogInformation("Orders paid via PayOS webhook, paymentCode={Code}", data.OrderCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but still return 200 — PayOS must not retry or mark URL as broken
                _logger.LogError(ex, "PayOS webhook processing error.");
            }

            return Ok(new { success = true });
        }
    }
}
