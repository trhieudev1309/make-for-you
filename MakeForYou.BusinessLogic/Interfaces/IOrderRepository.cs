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
        Task<List<Order>> FindByBuyerIdAsync(long buyerId);
        Task<Order?> GetOrderWithDetailsAsync(long orderId, long buyerId);
        Task<Order> AddAsync(Order order);
    }
}
