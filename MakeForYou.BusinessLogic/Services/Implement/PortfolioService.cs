using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepo;

        public PortfolioService(IPortfolioRepository portfolioRepo)
        {
            _portfolioRepo = portfolioRepo;
        }

        public async Task AddItemAsync(long sellerId, PortfolioItem item)
        {
            item.SellerId = sellerId;
            item.CreatedAt = DateTime.UtcNow;
            await _portfolioRepo.CreateAsync(item);
        }

        public async Task DeleteItemAsync(long sellerId, long portfolioId)
        {
            // Enforce ownership before deleting
            var item = await _portfolioRepo.FindByIdAsync(portfolioId);
            if (item == null || item.SellerId != sellerId) return;
            await _portfolioRepo.DeleteAsync(item);
        }

        public Task<List<PortfolioItem>> GetBySellerAsync(long sellerId) =>
            _portfolioRepo.GetBySellerAsync(sellerId);
    }
}