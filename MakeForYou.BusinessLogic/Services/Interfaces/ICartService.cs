using MakeForYou.BusinessLogic.Entities.DTOs.Respond;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(long? userId, long productId, int quantity);
        Task UpdateQuantityAsync(long? userId, long productId, int quantity); // Thêm hàm này
        Task RemoveItemAsync(long? userId, long productId);                 // Thêm hàm này
        Task<int> GetTotalItemsCountAsync(long? userId);
        Task<List<CartItemViewModel>> GetCartAsync(long? userId);
    }
}