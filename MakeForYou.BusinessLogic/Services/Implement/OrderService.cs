using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        private readonly IGhnService _ghnService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepo,
            ICartService cartService,
            ICartRepository cartRepo,
            IProductRepository productRepo,
            INotificationService notificationService,
            IProgressRepository progressRepo,
            IWebHostEnvironment env,
            IGhnService ghnService,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _cartService = cartService;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _notificationService = notificationService;
            _progressRepo = progressRepo;
            _env = env;
            _ghnService = ghnService;
            _logger = logger;
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
                Status = (int)OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var saved = await _orderRepo.AddAsync(order);

            // Send notification to seller
            await _notificationService.SendOrderNotificationAsync(saved);

            _logger.LogInformation("Order {OrderId} created: buyerId={BuyerId}, sellerId={SellerId}", saved.OrderId, buyerId, sellerId);
            return saved;
        }

        public async Task<List<Order>> CreateOrderFromCartAsync(long userId, CheckoutRequest request, long paymentCode)
        {
            // 1. Lấy toàn bộ giỏ hàng
            var cartItems = await _cartService.GetCartAsync(userId);
            if (!cartItems.Any()) throw new Exception("Giỏ hàng trống.");

            // 2. Thu thập dữ liệu Product để lấy thông tin phân tách theo từng Seller
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

            // 3. Nhóm giỏ hàng theo SellerId độc lập
            var groupedBySeller = itemsWithProduct.GroupBy(x => x.Product.SellerId);
            var createdOrders = new List<Order>();

            // 4. Chạy vòng lặp lập đơn: Mỗi nhóm Seller phát sinh 1 Order độc lập
            foreach (var group in groupedBySeller)
            {
                long sellerId = group.Key;
                var itemsInGroup = group.ToList();

                // Tính toán phí ship cho nhóm sản phẩm của Seller này từ GHN
                int sellerShippingFee = 0;
                if (request.ShippingDistrictId > 0 && !string.IsNullOrEmpty(request.ShippingWardCode))
                {
                    try
                    {
                        var productsForSeller = new List<Product>();
                        foreach (var x in itemsInGroup)
                        {
                            for (int i = 0; i < x.CartItem.Quantity; i++)
                            {
                                productsForSeller.Add(x.Product);
                            }
                        }

                        sellerShippingFee = await _ghnService.CalculateShippingFeeAsync(
                            productsForSeller,
                            request.ShippingDistrictId,
                            request.ShippingWardCode
                        );
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error calculating ship fee for seller {sellerId}: {ex.Message}");
                        sellerShippingFee = 30000; // Phí ship mặc định nếu lỗi kết nối GHN / Sandbox ShopId chưa cấu hình địa chỉ
                    }
                }

                // Khởi tạo thực thể Order và map chính xác cấu trúc địa chỉ sạch
                var order = new Order
                {
                    BuyerId = userId,
                    SellerId = sellerId,
                    CreatedAt = DateTime.UtcNow,
                    Status = (int)OrderStatus.Pending,
                    AgreedPrice = itemsInGroup.Sum(x => x.CartItem.TotalPrice),
                    PaymentCode = paymentCode,
                    IsPaid = false,

                    // ÁNH XẠ KHỚP HOÀN TOÀN VỚI ENTITY ORDER GỐC:
                    ShippingFullName = request.FullName,
                    ShippingPhone = request.PhoneNumber,
                    ShippingAddressDetail = request.ShippingAddressDetail,

                    ShippingProvinceId = request.ShippingProvinceId,
                    ShippingProvinceName = request.ShippingProvinceName,
                    ShippingDistrictId = request.ShippingDistrictId,
                    ShippingDistrictName = request.ShippingDistrictName,
                    ShippingWardCode = request.ShippingWardCode,
                    ShippingWardName = request.ShippingWardName,
                    ShippingFee = sellerShippingFee,

                    OrderDescription = $"Đơn hàng từ giỏ - Khách: {request.FullName} - ĐC: {request.ShippingAddressDetail}, {request.ShippingWardName}, {request.ShippingDistrictName}, {request.ShippingProvinceName}"
                };

                // Tạo danh sách OrderItem đính kèm cho đơn hàng này
                var orderItems = itemsInGroup.Select(x => new OrderItem
                {
                    ProductId = x.CartItem.ProductId,
                    Quantity = x.CartItem.Quantity,
                    Price = x.CartItem.Price,
                    CustomizationsJson = x.CartItem.CustomizationsJson
                }).ToList();

                // 5. Lưu vào Database
                var savedOrder = await _orderRepo.CreateOrderAsync(order, orderItems);
                createdOrders.Add(savedOrder);

                // Bắn thông báo thời gian thực tới Seller tương ứng
                await _notificationService.SendOrderNotificationAsync(savedOrder);
            }

            // 6. Dọn sạch giỏ hàng của Buyer
            await _cartRepo.ClearCartAsync(userId);

            _logger.LogInformation("{OrderCount} order(s) created from cart for buyer {BuyerId}", createdOrders.Count, userId);
            return createdOrders;
        }

        public async Task UpdateStatusAsync(long orderId, int status)
        {
            if (status == (int)OrderStatus.Cancelled)
            {
                try
                {
                    var order = await _orderRepo.GetOrderByIdAsync(orderId);
                    if (order != null && !string.IsNullOrEmpty(order.GhnShipmentCode))
                    {
                        var cancelResult = await _ghnService.CancelShipmentAsync(order.GhnShipmentCode);
                        if (!cancelResult)
                        {
                            _logger.LogError("Failed to cancel GHN shipment {GhnShipmentCode} for order {OrderId}.", order.GhnShipmentCode, orderId);
                        }
                        else
                        {
                            _logger.LogInformation("Successfully cancelled GHN shipment {GhnShipmentCode} for order {OrderId}.", order.GhnShipmentCode, orderId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cancelling GHN shipment for order {OrderId}.", orderId);
                }
            }

            // Gọi Repo để thực hiện cập nhật
            await _orderRepo.UpdateStatusAsync(orderId, status);
            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, (OrderStatus)status);
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
            {
                _logger.LogWarning("Progress update denied: order {OrderId} not found or seller {SellerId} does not own it", orderId, sellerId);
                return AuthResult.Fail("Order not found or access denied.");
            }

            // Guard: can't update a completed or cancelled order
            if (order.Status == (int)OrderStatus.Completed)
            {
                _logger.LogWarning("Progress update rejected: order {OrderId} is already completed", orderId);
                return AuthResult.Fail("This order is already completed.");
            }
            if (order.Status == (int)OrderStatus.Cancelled)
            {
                _logger.LogWarning("Progress update rejected: order {OrderId} is cancelled", orderId);
                return AuthResult.Fail("This order has been cancelled.");
            }

            // Guard: status must move forward only
            if (req.NewStatus <= order.Status)
            {
                _logger.LogWarning("Progress update rejected: order {OrderId} new status {NewStatus} is not ahead of current {CurrentStatus}", orderId, req.NewStatus, order.Status);
                return AuthResult.Fail("New status must be ahead of the current status.");
            }

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
            await UpdateStatusAsync(orderId, req.NewStatus);

            _logger.LogInformation("Order {OrderId} progress updated by seller {SellerId}: status={NewStatus}", orderId, sellerId, (OrderStatus)req.NewStatus);
            return AuthResult.Ok($"Order updated to {(OrderStatus)req.NewStatus}.");
        }

        public Task DropCustomizationAsync(long orderItemId) =>
            _orderRepo.ResolveCustomizationAsync(orderItemId);

        public async Task<long?> GetOrderIdByUsersAsync(long userA, long userB)
        {
            // userA là seller, userB là buyer
            var sellerOrders = await _orderRepo.FindBySellerIdAsync(userA);
            var order = sellerOrders.FirstOrDefault(o => o.BuyerId == userB);

            if (order == null)
            {
                // userB là seller, userA là buyer
                var sellerOrders2 = await _orderRepo.FindBySellerIdAsync(userB);
                order = sellerOrders2.FirstOrDefault(o => o.BuyerId == userA);
            }

            return order?.OrderId;
        }
    }
}