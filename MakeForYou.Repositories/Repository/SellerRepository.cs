using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    // ── Seller ──────────────────────────────────────────────────────────────
    public class SellerRepository : ISellerRepository
    {
        private readonly ApplicationDbContext _context;
        public SellerRepository(ApplicationDbContext context) => _context = context;

        public async Task<MakeForYou.BusinessLogic.Entities.Seller?> GetWithDetailsAsync(long sellerId) =>
            await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.PortfolioItems)
                .Include(s => s.Products)
                .Include(s => s.Reviews)
                .FirstOrDefaultAsync(s => s.SellerId == sellerId);

        public async Task UpdateAsync(MakeForYou.BusinessLogic.Entities.Seller seller)
        {
            _context.Sellers.Update(seller);
            await _context.SaveChangesAsync();
        }
    }

    // ── Portfolio ────────────────────────────────────────────────────────────
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _context;
        public PortfolioRepository(ApplicationDbContext context) => _context = context;

        public async Task<PortfolioItem?> FindByIdAsync(long portfolioId) =>
            await _context.Set<PortfolioItem>().FindAsync(portfolioId);

        public async Task<List<PortfolioItem>> GetBySellerAsync(long sellerId) =>
            await _context.Set<PortfolioItem>()
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task CreateAsync(PortfolioItem item)
        {
            _context.Set<PortfolioItem>().Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PortfolioItem item)
        {
            _context.Set<PortfolioItem>().Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}