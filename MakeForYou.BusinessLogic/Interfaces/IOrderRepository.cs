using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order, List<OrderItem> items);
        Task<Order?> GetOrderByIdAsync(long orderId);

        Task UpdateStatusAsync(long orderId, int status);
    }
}