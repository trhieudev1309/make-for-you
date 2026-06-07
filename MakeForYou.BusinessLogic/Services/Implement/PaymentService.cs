using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly PayOSClient _client;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IConfiguration config, ILogger<PaymentService> logger)
        {
            _logger = logger;
            _client = new PayOSClient(new PayOSOptions
            {
                ClientId = config["PayOS:ClientId"]
                    ?? throw new InvalidOperationException("PayOS:ClientId is not configured."),
                ApiKey = config["PayOS:ApiKey"]
                    ?? throw new InvalidOperationException("PayOS:ApiKey is not configured."),
                ChecksumKey = config["PayOS:ChecksumKey"]
                    ?? throw new InvalidOperationException("PayOS:ChecksumKey is not configured.")
            });
        }

        public async Task<string> CreatePaymentLinkAsync(long paymentCode, int amount, string baseUrl, IEnumerable<CartItemViewModel> items)
        {
            var description = $"MFY {paymentCode}";
            if (description.Length > 25)
                description = description[..25];

            var paymentItems = items.Select(i => new PaymentLinkItem
            {
                Name = (i.ProductName ?? "San pham").Length > 50
                    ? (i.ProductName ?? "San pham")[..50]
                    : (i.ProductName ?? "San pham"),
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            var request = new CreatePaymentLinkRequest
            {
                OrderCode = paymentCode,
                Amount = amount,
                Description = description,
                Items = paymentItems,
                CancelUrl = $"{baseUrl}/Checkout/Cancel?orderCode={paymentCode}",
                ReturnUrl = $"{baseUrl}/Checkout/Return?orderCode={paymentCode}"
            };

            var response = await _client.PaymentRequests.CreateAsync(request);
            _logger.LogInformation("Payment link created: paymentCode={PaymentCode}, amount={Amount}", paymentCode, amount);
            return response.CheckoutUrl;
        }

        public async Task<string> GetPaymentStatusAsync(long paymentCode)
        {
            var paymentLink = await _client.PaymentRequests.GetAsync(paymentCode);
            return paymentLink.Status.ToString();
        }

        public async Task<WebhookData> VerifyWebhookAsync(Webhook webhookBody)
        {
            return await _client.Webhooks.VerifyAsync(webhookBody);
        }
    }
}
