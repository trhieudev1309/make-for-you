using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V1.Payouts;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class PayoutService : IPayoutService
    {
        private readonly ApplicationDbContext _db;
        private readonly PayOSClient _client;

        public PayoutService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
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
                return (false, "Không tìm thấy đơn hàng.");

            if (order.Status != (int)OrderStatus.Done)
                return (false, "Đơn hàng chưa ở trạng thái Đã xong.");

            if (order.IsSellerPaid)
                return (true, "Nghệ nhân đã được thanh toán trước đó.");

            if (!order.AgreedPrice.HasValue || order.AgreedPrice <= 0)
                return (false, "Đơn hàng chưa có giá thanh toán.");

            var seller = order.Seller;
            if (string.IsNullOrWhiteSpace(seller?.BankBin) || string.IsNullOrWhiteSpace(seller.BankAccountNumber))
                return (false, "Nghệ nhân chưa cài đặt thông tin tài khoản ngân hàng.");

            var referenceId = $"MFY-{orderId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var idempotencyKey = Guid.NewGuid().ToString();

            var desc = $"Don hang #{orderId}";
            if (desc.Length > 50) desc = desc[..50];

            var payoutRequest = new PayoutRequest
            {
                ReferenceId = referenceId,
                Amount = order.AgreedPrice.Value,
                Description = desc,
                ToBin = seller.BankBin,
                ToAccountNumber = seller.BankAccountNumber
            };

            try
            {
                var result = await _client.Payouts.CreateAsync(payoutRequest, idempotencyKey);

                order.IsSellerPaid = true;
                order.PayoutReferenceId = result?.Id ?? referenceId;
                await _db.SaveChangesAsync();

                return (true, $"Đã chuyển {order.AgreedPrice.Value:N0} VNĐ cho nghệ nhân thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Chuyển tiền thất bại: {ex.Message}");
            }
        }
    }
}
