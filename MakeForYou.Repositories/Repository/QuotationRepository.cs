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

        public async Task<Quotation?> GetByIdAsync(long id) =>
            await _context.Set<Quotation>()
                .FirstOrDefaultAsync(q => q.QuotationId == id);

        // UPDATE status
        public async Task UpdateStatusAsync(long id, int status)
        {
            var q = await _context.Set<Quotation>().FindAsync(id)
                ?? throw new KeyNotFoundException($"Quotation {id} not found.");

            q.Status = status;
            await _context.SaveChangesAsync();
        }
    }
}