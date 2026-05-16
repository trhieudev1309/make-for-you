using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IQuotationService
    {
        Task CreateAsync(Quotation quotation);
        Task<List<Quotation>> GetByOrderAsync(long orderId);
        Task AcceptAsync(long quotationId, long buyerId);
        Task RejectAsync(long quotationId, long buyerId);
        Task ConfirmAsync(long quotationId, long sellerId);
    }
}