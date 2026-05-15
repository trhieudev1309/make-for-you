using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs;
using MakeForYou.BusinessLogic.Enums;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartService _cartService;
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly INotificationService _notificationService;
        private readonly IProgressRepository _progressRepo;
        private readonly IWebHostEnvironment _env;

        public OrderService(
            IOrderRepository orderRepo,
            ICartService cartService,
            ICartRepository cartRepo,
            IProductRepository productRepo,
            INotificationService notificationService)
            IProductRepository productRepo,
            IProgressRepository progressRepo,
            IWebHostEnvironment env) // Thêm tham số này
        {
            _orderRepo = orderRepo;
            _cartService = cartService;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _notificationService = notificationService;
            _productRepo = productRepo; // Gán giá trị vào biến private
            _progressRepo = progressRepo; // Gán giá trị vào biến private
            _env = env; // Gán giá trị vào biến private
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

        public Task<List<Order>> GetRequestsBySellerAsync(long sellerId) =>
    _orderRepo.FindBySellerIdAsync(sellerId);

        public Task<Order?> GetRequestDetailAsync(long orderId, long sellerId) =>
            _orderRepo.GetOrderWithDetailsBySellerAsync(orderId, sellerId);
    

    public Task<Order?> GetOrderForSellerAsync(long orderId, long sellerId) =>
    _orderRepo.GetOrderForSellerAsync(orderId, sellerId);

        public async Task<AuthResult> UpdateProgressAsync(long orderId, long sellerId, UpdateProgressRequest req)
        {
            var order = await _orderRepo.GetOrderForSellerAsync(orderId, sellerId);
            if (order == null)
                return AuthResult.Fail("Order not found or access denied.");

            // Guard: can't update a completed or cancelled order
            if (order.Status == (int)OrderStatus.Completed)
                return AuthResult.Fail("This order is already completed.");
            if (order.Status == (int)OrderStatus.Cancelled)
                return AuthResult.Fail("This order has been cancelled.");

            // Guard: status must move forward only
            if (req.NewStatus <= order.Status)
                return AuthResult.Fail("New status must be ahead of the current status.");

            // 1. Save image if uploaded
            string? imageUrl = null;
            if (req.Image != null && req.Image.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(req.Image.FileName).ToLower();
                if (!allowed.Contains(ext))
                    return AuthResult.Fail("Only JPG, PNG, or WEBP images are allowed.");

                var folder = Path.Combine(_env.WebRootPath, "uploads", "progress");
                Directory.CreateDirectory(folder);
                var fileName = $"{orderId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await req.Image.CopyToAsync(stream);
                imageUrl = $"/uploads/progress/{fileName}";
            }

            // 2. Insert progress log
            await _progressRepo.CreateAsync(new OrderProgress
            {
                OrderId = orderId,
                Note = req.Note.Trim(),
                ImageUrl = imageUrl,
                StatusSnapshot = req.NewStatus,
                CreatedAt = DateTime.UtcNow
            });

            // 3. Update order status
            await _orderRepo.UpdateStatusAsync(orderId, req.NewStatus);

            return AuthResult.Ok($"Order updated to {(OrderStatus)req.NewStatus}.");
        }
    }
}