using PayOS.Models.Webhooks;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentLinkAsync(long paymentCode, int amount, string baseUrl);
        Task<string> GetPaymentStatusAsync(long paymentCode);
        Task<WebhookData> VerifyWebhookAsync(Webhook webhookBody);
    }
}
