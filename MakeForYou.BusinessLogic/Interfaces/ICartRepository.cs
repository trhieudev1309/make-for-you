using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface ICartRepository
    {
        Task<CartItem?> GetExistingItemAsync(long userId, long productId);
        Task AddItemAsync(CartItem item);
        Task UpdateItemAsync(CartItem item);
        Task DeleteItemAsync(CartItem item); // Thêm hàm này
        Task<int> GetCountByUserIdAsync(long userId);
        Task<List<CartItem>> GetItemsByUserIdAsync(long userId);

        Task ClearCartAsync(long userId);
    }
}