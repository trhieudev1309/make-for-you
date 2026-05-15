using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepo;
        public QuotationService(IQuotationRepository quotationRepo) => _quotationRepo = quotationRepo;

        public Task CreateAsync(Quotation quotation) => _quotationRepo.CreateAsync(quotation);
        public Task<List<Quotation>> GetByOrderAsync(long orderId) => _quotationRepo.GetByOrderAsync(orderId);
    }
}