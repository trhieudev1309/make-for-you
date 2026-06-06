using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IQuotationService
    {
        Task CreateAsync(Quotation quotation);
        Task<List<Quotation>> GetByOrderAsync(long orderId);
        Task ApproveAsync(long quotationId, long buyerId);
        Task CancelAsync(long quotationId, long userId);
        Task<Quotation?> GetByIdAsync(long quotationId);
    }
}