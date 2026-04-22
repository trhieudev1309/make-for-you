using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.Repositories.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context) => _context = context;

        // Order history — all orders for a buyer, newest first
        public async Task<List<Order>> FindByBuyerIdAsync(long buyerId) =>
            await _context.Orders
                     .Where(o => o.BuyerId == buyerId)
                     .Include(o => o.Seller).ThenInclude(s => s.User)
                     .Include(o => o.Quotations)
                     .OrderByDescending(o => o.CreatedAt)
                     .ToListAsync();

        // Order detail — enforces ownership (buyerId must match)
        public async Task<Order?> GetOrderWithDetailsAsync(long orderId, long buyerId) =>
            await _context.Orders
                     .Where(o => o.OrderId == orderId && o.BuyerId == buyerId)
                     .Include(o => o.Seller).ThenInclude(s => s.User)
                     .Include(o => o.Quotations)
                     .Include(o => o.Reviews)
                     .Include(o => o.ChatMessages.OrderBy(m => m.SentAt))
                         .ThenInclude(m => m.Sender)
                     .FirstOrDefaultAsync();

        public async Task<Order> AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
    }
}
