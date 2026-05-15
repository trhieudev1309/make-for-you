using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly ApplicationDbContext _context;
        public QuotationRepository(ApplicationDbContext context) => _context = context;

        public async Task CreateAsync(Quotation quotation)
        {
            _context.Set<Quotation>().Add(quotation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Quotation>> GetByOrderAsync(long orderId) =>
            await _context.Set<Quotation>()
                .Where(q => q.OrderId == orderId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
    }
}