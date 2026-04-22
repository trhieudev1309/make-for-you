using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        public OrderService(IOrderRepository orderRepo) => _orderRepo = orderRepo;

        public Task<List<Order>> GetOrdersByUserAsync(long buyerId) =>
            _orderRepo.FindByBuyerIdAsync(buyerId);

        public Task<Order?> GetOrderDetailAsync(long orderId, long buyerId) =>
            _orderRepo.GetOrderWithDetailsAsync(orderId, buyerId);

        public async Task<Order> CreateOrderAsync(long buyerId, long sellerId, string description)
        {
            var order = new Order
            {
                BuyerId = buyerId,
                SellerId = sellerId,
                OrderDescription = description,
                Status = (int)MakeForYou.BusinessLogic.Enums.OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            return await _orderRepo.AddAsync(order);
        }
    }
}
