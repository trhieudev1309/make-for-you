using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
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
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

        public async Task<Order> AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> CreateOrderAsync(Order order, List<OrderItem> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var maxOrderId = await _context.Orders.MaxAsync(o => (long?)o.OrderId) ?? 0;
                order.OrderId = maxOrderId + 1;

                var maxItemId = await _context.Set<OrderItem>().MaxAsync(i => (long?)i.OrderItemId) ?? 0;

                _context.Set<Order>().Add(order);
                await _context.SaveChangesAsync();

                for (int i = 0; i < items.Count; i++)
                {
                    items[i].OrderItemId = maxItemId + 1 + i;
                    items[i].OrderId = order.OrderId;
                    _context.Set<OrderItem>().Add(items[i]);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch (Exception)
            {
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

        public async Task UpdateStatusAsync(long orderId, int newStatus)
        {
            var order = await _context.Set<Order>().FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;

                // Sử dụng enum OrderStatus.Completed từ hệ thống để đồng bộ logic
                if (newStatus == (int)OrderStatus.Completed)
                {
                    order.CompletedAt = DateTime.UtcNow;
                }
                else
                {
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

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

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

        public async Task<List<Order>> FindBySellerIdAsync(long sellerId) =>
            await _context.Orders
                .Where(o => o.SellerId == sellerId)
                .Include(o => o.Buyer)
                .Include(o => o.Quotations)
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

        public async Task<Order?> GetOrderWithDetailsBySellerAsync(long orderId, long sellerId) =>
    await _context.Orders
        .Where(o => o.OrderId == orderId && o.SellerId == sellerId)
        .Include(o => o.Buyer)
        .Include(o => o.Seller).ThenInclude(s => s.User)
        .Include(o => o.Quotations)
        .Include(o => o.OrderItems).ThenInclude(i => i.Product)
        .Include(o => o.ChatMessages.OrderBy(m => m.CreatedAt)) // Sửa từ SentAt thành CreatedAt
            .ThenInclude(m => m.FromUser)                       // Sửa từ Sender thành FromUser
        .Include(o => o.Reviews)
        .FirstOrDefaultAsync();

        public async Task<Order?> GetOrderForSellerAsync(long orderId, long sellerId) =>
            await _context.Orders
                .Where(o => o.OrderId == orderId && o.SellerId == sellerId)
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .Include(o => o.ProgressLogs)
                .FirstOrDefaultAsync();

        public async Task UpdatePaymentStatusByCodeAsync(long paymentCode, bool isPaid)
        {
            var orders = await _context.Orders
                .Where(o => o.PaymentCode == paymentCode)
                .Include(o => o.OrderItems)
                .Include(o => o.Quotations)
                .ToListAsync();

            // Statuses that legitimately expect a payment webhook.
            // Any other status means the order has already been processed — skip it so
            // a duplicate webhook call (or a webhook arriving after a manual fix) cannot
            // corrupt an order that is already past the payment gate.
            var awaitingPayment = new[]
            {
                (int)OrderStatus.Pending,
                (int)OrderStatus.PendingQuotationPayment
            };

            foreach (var order in orders)
            {
                order.IsPaid = isPaid;

                if (!isPaid)
                    continue; // payment failed/reversed — just mark as unpaid, leave status alone

                // Skip orders that are not in a payment-awaiting state.
                // NOTE: do NOT use order.IsPaid for this check — cart orders already
                // have IsPaid=true from the initial checkout, yet they can legitimately
                // be here again when the buyer pays a quotation add-on.
                if (!awaitingPayment.Contains(order.Status))
                    continue;

                if (order.Status == (int)OrderStatus.PendingQuotationPayment)
                {
                    // Buyer paid after accepting a quotation — resolve customisations and confirm
                    if (order.OrderItems != null)
                        foreach (var item in order.OrderItems)
                            item.IsCustomizationResolved = true;

                    order.Status = (int)OrderStatus.Confirmed;
                }
                else
                {
                    // Initial checkout payment — route based on whether items need quotation
                    bool needsQuotation = order.OrderItems?.Any(i => i.HasCustomization) ?? false;
                    order.Status = needsQuotation
                        ? (int)OrderStatus.PendingQuotationSubmit
                        : (int)OrderStatus.Confirmed;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> FindByPaymentCodeAsync(long paymentCode) =>
            await _context.Orders
                .Where(o => o.PaymentCode == paymentCode)
                .Include(o => o.Quotations)
                .ToListAsync();

        public async Task ResolveCustomizationAsync(long orderItemId)
        {
            var item = await _context.Set<OrderItem>().FindAsync(orderItemId);
            if (item != null)
            {
                item.IsCustomizationResolved = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ResolveAllCustomizationsAsync(long orderId)
        {
            var items = await _context.Set<OrderItem>()
                .Where(i => i.OrderId == orderId && i.HasCustomization && !i.IsCustomizationResolved)
                .ToListAsync();
            foreach (var item in items)
                item.IsCustomizationResolved = true;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAgreedPriceAsync(long orderId, int newPrice)
        {
            var order = await _context.Set<Order>().FindAsync(orderId);
            if (order != null)
            {
                order.AgreedPrice = newPrice;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateGhnShipmentCodeAsync(long orderId, string ghnShipmentCode)
        {
            var order = await _context.Set<Order>().FindAsync(orderId);
            if (order != null)
            {
                order.GhnShipmentCode = ghnShipmentCode;
                await _context.SaveChangesAsync();
            }
        }
    }
}