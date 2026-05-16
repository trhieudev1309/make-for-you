using MakeForYou.BusinessLogic.Entities;
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

        // ── có sẵn ────────────────────────────────────────────────────────────
        public Task CreateAsync(Quotation quotation) => _quotationRepo.CreateAsync(quotation);
        public Task<List<Quotation>> GetByOrderAsync(long orderId) => _quotationRepo.GetByOrderAsync(orderId);

        // ── Buyer accept ──────────────────────────────────────────────────────
        public async Task AcceptAsync(long quotationId, long buyerId)
        {
            var (q, order) = await Load(quotationId);

            if (order.BuyerId != buyerId)
                throw new UnauthorizedAccessException("Only the buyer can accept this quotation.");
            if (q.Status != 0)
                throw new InvalidOperationException("Only a pending quotation can be accepted.");

            await _quotationRepo.UpdateStatusAsync(quotationId, 1);
        }

        // ── Buyer reject ──────────────────────────────────────────────────────
        public async Task RejectAsync(long quotationId, long buyerId)
        {
            var (q, order) = await Load(quotationId);

            if (order.BuyerId != buyerId)
                throw new UnauthorizedAccessException("Only the buyer can reject this quotation.");
            if (q.Status != 0)
                throw new InvalidOperationException("Only a pending quotation can be rejected.");

            await _quotationRepo.UpdateStatusAsync(quotationId, 2);
        }

        // ── Seller confirm (sau khi Buyer accept) ─────────────────────────────
        public async Task ConfirmAsync(long quotationId, long sellerId)
        {
            var (q, order) = await Load(quotationId);

            if (order.SellerId != sellerId)
                throw new UnauthorizedAccessException("Only the seller can confirm this quotation.");
            if (q.Status != 1)
                throw new InvalidOperationException("Can only confirm an accepted quotation.");

            await _quotationRepo.UpdateStatusAsync(quotationId, 3);
        }

        // ── helper ────────────────────────────────────────────────────────────
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