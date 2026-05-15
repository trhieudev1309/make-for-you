using MakeForYou.BusinessLogic.Entities.DTOs.Respond;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<bool> CreateCategoryAsync(string name);
        // Thêm hàm Update này vào
        Task<bool> UpdateCategoryAsync(long id, string name);
        Task<bool> DeleteCategoryAsync(long id);
    }
}