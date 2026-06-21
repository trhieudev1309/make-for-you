using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.V1.Payouts;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class PayoutService : IPayoutService
    {
        private readonly ApplicationDbContext _db;
        private readonly PayOSClient _client;
        private readonly ILogger<PayoutService> _logger;

        public PayoutService(ApplicationDbContext db, IConfiguration config, ILogger<PayoutService> logger)
        {
            _db = db;
            _logger = logger;
            _client = new PayOSClient(new PayOSOptions
            {
                ClientId = config["PayOS:Pay_ClientId"]
                   ?? throw new InvalidOperationException("PayOS:Pay_ClientId is not configured."),
                ApiKey = config["PayOS:Pay_ApiKey"]
                   ?? throw new InvalidOperationException("PayOS:Pay_ApiKey is not configured."),
                ChecksumKey = config["PayOS:Pay_ChecksumKey"]
                   ?? throw new InvalidOperationException("PayOS:Pay_ChecksumKey is not configured.")
            });
        }

        public async Task<(bool Success, string Message)> PaySellerAsync(long orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Seller)
                .Include(o => o.Quotations)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                _logger.LogWarning("Payout skipped: order {OrderId} not found", orderId);
                return (false, "Không tìm thấy đơn hàng.");
            }

            if (order.Status != (int)OrderStatus.Done)
            {
                _logger.LogWarning("Payout skipped: order {OrderId} is not in Done status (current={Status})", orderId, order.Status);
                return (false, "Đơn hàng chưa ở trạng thái Đã xong.");
            }

            if (order.IsSellerPaid)
            {
                _logger.LogInformation("Payout skipped: seller for order {OrderId} was already paid", orderId);
                return (true, "Nghệ nhân đã được thanh toán trước đó.");
            }

            if (!order.AgreedPrice.HasValue || order.AgreedPrice <= 0)
            {
                _logger.LogWarning("Payout skipped: order {OrderId} has no agreed price", orderId);
                return (false, "Đơn hàng chưa có giá thanh toán.");
            }

            var seller = order.Seller;
            if (string.IsNullOrWhiteSpace(seller?.BankBin) || string.IsNullOrWhiteSpace(seller.BankAccountNumber))
            {
                _logger.LogWarning("Payout skipped: seller {SellerId} for order {OrderId} has no bank account configured", order.SellerId, orderId);
                return (false, "Nghệ nhân chưa cài đặt thông tin tài khoản ngân hàng.");
            }

            // Payout includes the agreed price PLUS the shipping fee — the seller fronts
            // the shipment cost via GHN, so it must be reimbursed alongside the order amount.
            var payoutAmount = order.AgreedPrice.Value + (order.ShippingFee ?? 0);

            var referenceId = $"MFY-{orderId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var idempotencyKey = Guid.NewGuid().ToString();

            var desc = $"Don hang #{orderId}";
            if (desc.Length > 50) desc = desc[..50];

            var payoutRequest = new PayoutRequest
            {
                ReferenceId = referenceId,
                Amount = payoutAmount,
                Description = desc,
                ToBin = seller.BankBin,
                ToAccountNumber = seller.BankAccountNumber
            };

            _logger.LogInformation("Initiating payout for order {OrderId}: sellerId={SellerId}, amount={Amount}", orderId, order.SellerId, payoutAmount);

            try
            {
                var result = await _client.Payouts.CreateAsync(payoutRequest, idempotencyKey);

                order.IsSellerPaid = true;
                order.PayoutReferenceId = result?.Id ?? referenceId;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payout successful for order {OrderId}: referenceId={ReferenceId}, amount={Amount}", orderId, order.PayoutReferenceId, payoutAmount);
                return (true, $"Đã chuyển {payoutAmount:N0} VNĐ cho nghệ nhân thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payout failed for order {OrderId}: referenceId={ReferenceId}", orderId, referenceId);
                throw;
            }
        }
    }
}
