using MakeForYou.BusinessLogic.Entities;

public interface IProgressRepository
{
    Task<OrderProgress> CreateAsync(OrderProgress progress);
    Task<List<OrderProgress>> GetByOrderIdAsync(long orderId);
}