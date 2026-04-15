using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> SearchAsync(string? keyword, long? categoryId);
        Task<List<Category>> GetCategoriesAsync();
        Task<Product?> FindByIdAsync(long id);
    }
}