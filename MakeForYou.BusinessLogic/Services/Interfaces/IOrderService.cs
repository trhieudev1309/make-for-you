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
    }
}
