using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }
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
             .Include(o => o.ChatMessages.OrderBy(m => m.CreatedAt))
                 .ThenInclude(m => m.FromUser)
             .Include(o => o.OrderItems)          // ← add this
                 .ThenInclude(i => i.Product)     // ← and this
             .FirstOrDefaultAsync();

        public async Task<Order> AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
        public async Task<Order> CreateOrderAsync(Order order, List<OrderItem> items)
        {
            // Bắt đầu giao dịch (Transaction)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lưu đơn hàng chính
                _context.Set<Order>().Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy được OrderId tự tăng

                // 2. Gán OrderId cho từng item và lưu vào bảng OrderItems
                foreach (var item in items)
                {
                    item.OrderId = order.OrderId;
                    _context.Set<OrderItem>().Add(item);
                }

                await _context.SaveChangesAsync();

                // 3. Xác nhận mọi thứ thành công
                await transaction.CommitAsync();
                return order;
            }
            catch (Exception)
            {
                // Nếu có bất kỳ lỗi gì, hủy bỏ toàn bộ thay đổi
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(long orderId)
        {
            return await _context.Set<Order>()
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task UpdateStatusAsync(long orderId, int status)
        {
            var order = await _context.Set<Order>().FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
        }
    }
}