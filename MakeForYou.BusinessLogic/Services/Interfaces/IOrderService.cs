using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request; // Giả định bạn có Dto cho Checkout

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> CreateOrderFromCartAsync(long userId, string fullName, string phone, string address);

        Task UpdateStatusAsync(long orderId, int status);
    }
}