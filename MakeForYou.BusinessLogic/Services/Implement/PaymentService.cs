using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly PayOSClient _client;

        public PaymentService(IConfiguration config)
        {
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

        public async Task<string> CreatePaymentLinkAsync(long paymentCode, int amount, string baseUrl)
        {
            var description = $"MFY {paymentCode}";
            if (description.Length > 25)
                description = description[..25];

            var request = new CreatePaymentLinkRequest
            {
                OrderCode = paymentCode,
                Amount = amount,
                Description = description,
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem { Name = "Don hang", Quantity = 1, Price = amount }
                },
                CancelUrl = $"{baseUrl}/Checkout/Cancel?orderCode={paymentCode}",
                ReturnUrl = $"{baseUrl}/Checkout/Return?orderCode={paymentCode}"
            };

            var response = await _client.PaymentRequests.CreateAsync(request);
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
