using MakeForYou.BusinessLogic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order, List<OrderItem> items);
        Task<Order?> GetOrderByIdAsync(long orderId);
        Task UpdateStatusAsync(long orderId, int status);
        Task<List<Order>> FindByBuyerIdAsync(long buyerId);
        Task<Order?> GetOrderWithDetailsAsync(long orderId, long buyerId);
        Task<Order> AddAsync(Order order);
        Task<(List<Order> Orders, int TotalCount)> GetAllOrdersFilteredAsync(
        string? search, int? status, int pageIndex, int pageSize);
    }
}
