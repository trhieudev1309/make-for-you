using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> SearchAsync(string? keyword, long? categoryId);
        Task<List<Category>> GetCategoriesAsync();
    }
}