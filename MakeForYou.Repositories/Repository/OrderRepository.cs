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

                // KIỂM TRA: Nếu chuyển thành Completed (4) thì cập nhật giờ hoàn thành
                if (status == 4)
                {
                    order.CompletedAt = DateTime.UtcNow; // Hoặc DateTime.Now tùy múi giờ dự án của bạn
                }
                else
                {
                    // Nếu lỡ chuyển nhầm thành Completed rồi chuyển lại Pending, thì clear CompletedAt đi
                    order.CompletedAt = null;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Order> Orders, int TotalCount)> GetAllOrdersFilteredAsync(
    string? search, int? status, int pageIndex, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller).ThenInclude(s => s.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Lọc theo trạng thái
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            // Tìm kiếm theo mã đơn hoặc tên khách
            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.OrderId.ToString().Contains(search) || o.Buyer.FullName.Contains(search));

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }
    }
}