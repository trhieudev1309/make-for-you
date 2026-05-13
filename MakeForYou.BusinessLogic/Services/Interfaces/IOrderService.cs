using MakeForYou.BusinessLogic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersByUserAsync(long buyerId);
        Task<Order?> GetOrderDetailAsync(long orderId, long buyerId);
        Task<Order> CreateOrderAsync(long buyerId, long sellerId, string description);
        Task<List<Order>> CreateOrderFromCartAsync(long userId, string fullName, string phone, string address);

        Task UpdateStatusAsync(long orderId, int status);

        Task<(List<Order> Orders, int TotalCount)> GetAllOrdersForAdminAsync(
        string? search, int? status, int pageIndex, int pageSize);
    }
}
