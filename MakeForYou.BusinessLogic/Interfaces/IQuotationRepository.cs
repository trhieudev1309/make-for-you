using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface IQuotationRepository
    {
        Task CreateAsync(Quotation quotation);
        Task<List<Quotation>> GetByOrderAsync(long orderId);
    }
}