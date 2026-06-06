using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepo;
        private readonly IOrderRepository _orderRepo;

        public QuotationService(IQuotationRepository quotationRepo, IOrderRepository orderRepo)
        {
            _quotationRepo = quotationRepo;
            _orderRepo = orderRepo;
        }

        public Task CreateAsync(Quotation quotation) => _quotationRepo.CreateAsync(quotation);
        public Task<List<Quotation>> GetByOrderAsync(long orderId) => _quotationRepo.GetByOrderAsync(orderId);
        public Task<Quotation?> GetByIdAsync(long quotationId) => _quotationRepo.GetByIdAsync(quotationId);

        // ── Buyer approves ────────────────────────────────────────────────────
        public async Task ApproveAsync(long quotationId, long buyerId)
        {
            var (q, order) = await Load(quotationId);

            if (order.BuyerId != buyerId)
                throw new UnauthorizedAccessException("Only the buyer can approve this quotation.");
            if (q.Status != 0)
                throw new InvalidOperationException("Only a pending quotation can be approved.");

            // 1. Mark quotation as approved
            await _quotationRepo.UpdateStatusAsync(quotationId, 1);

            // 2. Update agreed price on the order
            //    - Commission order (AgreedPrice is null): ProposedPrice becomes the total
            //    - Add-on / cart-customisation order: add ProposedPrice on top of existing total
            if (q.ProposedPrice.HasValue)
            {
                var newPrice = (order.AgreedPrice ?? 0) + q.ProposedPrice.Value;
                await _orderRepo.UpdateAgreedPriceAsync(order.OrderId, newPrice);
            }

            // 3. Gate on payment — buyer must pay before the order can proceed
            await _orderRepo.UpdateStatusAsync(order.OrderId, (int)OrderStatus.PendingQuotationPayment);
        }

        // ── Either party cancels ──────────────────────────────────────────────
        public async Task CancelAsync(long quotationId, long userId)
        {
            var (q, order) = await Load(quotationId);

            var isCreator = q.CreatedBy == userId;
            var isBuyer   = order.BuyerId == userId;

            if (!isCreator && !isBuyer)
                throw new UnauthorizedAccessException("Only the seller or buyer can cancel this quotation.");
            if (q.Status != 0)
                throw new InvalidOperationException("Only a pending quotation can be cancelled.");

            await _quotationRepo.UpdateStatusAsync(quotationId, 2);
        }

        private async Task<(Quotation q, Order order)> Load(long quotationId)
        {
            var q = await _quotationRepo.GetByIdAsync(quotationId)
                ?? throw new KeyNotFoundException($"Quotation {quotationId} not found.");

            var order = await _orderRepo.GetOrderByIdAsync(q.OrderId)
                ?? throw new KeyNotFoundException($"Order {q.OrderId} not found.");

            return (q, order);
        }
    }
}
