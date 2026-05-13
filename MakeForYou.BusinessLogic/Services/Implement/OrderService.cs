using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Enums;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartService _cartService;
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly INotificationService _notificationService;

        public OrderService(
            IOrderRepository orderRepo,
            ICartService cartService,
            ICartRepository cartRepo,
            IProductRepository productRepo,
            INotificationService notificationService)
        {
            _orderRepo = orderRepo;
            _cartService = cartService;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _notificationService = notificationService;
        }
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
            var saved = await _orderRepo.AddAsync(order);

            // Send notification to seller
            await _notificationService.SendOrderNotificationAsync(saved);

            return saved;
        }

        public async Task<List<Order>> CreateOrderFromCartAsync(long userId, string fullName, string phone, string address)
        {
            // 1. Lấy toàn bộ giỏ hàng
            var cartItems = await _cartService.GetCartAsync(userId);
            if (!cartItems.Any()) throw new Exception("Giỏ hàng trống.");

            // 2. Vì CartItemViewModel có thể chưa có SellerId, ta cần lấy thông tin Seller cho từng món
            // Để tối ưu, ta lấy thông tin Product từ Repo
            var itemsWithProduct = new List<dynamic>();

            foreach (var item in cartItems)
            {
                var product = await _productRepo.FindByIdAsync(item.ProductId);

                itemsWithProduct.Add(new
                {
                    CartItem = item,
                    Product = product
                });
            }

            // 3. NHÓM THEO SELLERID
            var groupedBySeller = itemsWithProduct.GroupBy(x => x.Product.SellerId);

            var createdOrders = new List<Order>();

            // 4. CHẠY VÒNG LẶP: Mỗi Seller = 1 Đơn hàng
            foreach (var group in groupedBySeller)
            {
                long sellerId = group.Key;
                var itemsInGroup = group.ToList();

                // Tạo đối tượng Order cho từng Seller
                var order = new Order
                {
                    BuyerId = userId,
                    SellerId = sellerId, // Gán SellerId đúng cho từng đơn
                    CreatedAt = DateTime.Now,
                    Status = (int)OrderStatus.Pending,
                    AgreedPrice = itemsInGroup.Sum(x => x.CartItem.TotalPrice),
                    OrderDescription = $"Đơn hàng từ giỏ - Khách: {fullName} - ĐC: {address}"
                };

                // Tạo danh sách OrderItem cho đơn hàng này
                var orderItems = itemsInGroup.Select(x => new OrderItem
                {
                    ProductId = x.CartItem.ProductId,
                    Quantity = x.CartItem.Quantity,
                    Price = x.CartItem.Price
                }).ToList();

                // 5. Lưu vào Database (Mỗi đơn 1 lần gọi Repo)
                var savedOrder = await _orderRepo.CreateOrderAsync(order, orderItems);
                createdOrders.Add(savedOrder);

                // Send notification to seller for each created order
                await _notificationService.SendOrderNotificationAsync(savedOrder);
            }

            // 6. Xóa sạch giỏ hàng sau khi đã tách và lưu hết các đơn
            await _cartRepo.ClearCartAsync(userId);

            return createdOrders;
        }

        public async Task UpdateStatusAsync(long orderId, int status)
        {
            // Gọi Repo để thực hiện cập nhật
            await _orderRepo.UpdateStatusAsync(orderId, status);
        }

        public Task<(List<Order> Orders, int TotalCount)> GetAllOrdersForAdminAsync(
    string? search, int? status, int pageIndex, int pageSize)
        {
            return _orderRepo.GetAllOrdersFilteredAsync(search, status, pageIndex, pageSize);
        }
    }
}