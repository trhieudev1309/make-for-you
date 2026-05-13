using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(long id);
        Task<bool> AddAsync(Category category);
        Task<bool> UpdateAsync(Category category);
        Task<bool> DeleteAsync(long id);
    }
}