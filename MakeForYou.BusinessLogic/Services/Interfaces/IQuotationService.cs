using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IQuotationService
    {
        Task CreateAsync(Quotation quotation);
        Task<List<Quotation>> GetByOrderAsync(long orderId);
    }
}