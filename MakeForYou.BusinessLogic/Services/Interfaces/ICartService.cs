using MakeForYou.BusinessLogic.Entities.DTOs.Respond;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface ICartService
    {
        // Updated to accept customizations
        Task AddToCartAsync(long? userId, long productId, int quantity, string? customizationsJson = null);

        Task UpdateQuantityAsync(long? userId, long productId, int quantity);

        Task RemoveItemAsync(long? userId, long productId);

        Task<int> GetTotalItemsCountAsync(long? userId);

        Task<List<CartItemViewModel>> GetCartAsync(long? userId);
    }
}