using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface ISellerRepository
    {
        Task<MakeForYou.BusinessLogic.Entities.Seller?> GetWithDetailsAsync(long sellerId);
        Task UpdateAsync(MakeForYou.BusinessLogic.Entities.Seller seller);
    }

    public interface IPortfolioRepository
    {
        Task<PortfolioItem?> FindByIdAsync(long portfolioId);
        Task<List<PortfolioItem>> GetBySellerAsync(long sellerId);
        Task CreateAsync(PortfolioItem item);
        Task DeleteAsync(PortfolioItem item);
    }
}