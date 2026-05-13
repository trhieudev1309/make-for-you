using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Hubs; // for NotificationHub
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IEmailService emailService,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        // Notify the seller and buyer when a new order is created
        public async Task SendOrderNotificationAsync(Order order)
        {
            if (order == null) return;

            // Seller user (SellerId maps to the user's id)
            var sellerUser = await _userRepo.FindByIdAsync(order.SellerId);
            var buyerUser = await _userRepo.FindByIdAsync(order.BuyerId);

            // --- Seller notification ---
            var sellerTitle = $"New order #{order.OrderId}";
            var sellerMessage = $"You have received a new order (#{order.OrderId}) from {buyerUser?.FullName ?? "a buyer"}.";

            var sellerNotification = new Notification
            {
                UserId = order.SellerId,
                OrderId = order.OrderId,
                Title = sellerTitle,
                Message = sellerMessage,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            var savedSellerNotification = await _notificationRepo.CreateAsync(sellerNotification);

            // Real-time push to connected seller (relies on SignalR User identifier mapping)
            try
            {
                var payload = new
                {
                    id = savedSellerNotification.NotificationId,
                    title = savedSellerNotification.Title,
                    message = savedSellerNotification.Message,
                    orderId = savedSellerNotification.OrderId,
                    createdAt = savedSellerNotification.CreatedAt,
                    isRead = savedSellerNotification.IsRead
                };

                // Send to seller by user id (string). SignalR uses IUserIdProvider (by default NameIdentifier claim).
                await _hubContext.Clients.User(savedSellerNotification.UserId.ToString()).SendAsync("ReceiveNotification", payload);
            }
            catch
            {
                // Ignore hub errors so notification persistence is the main success path
            }

            // Optional: send email to seller as well if configured
            if (!string.IsNullOrWhiteSpace(sellerUser?.Email))
            {
                try
                {
                    var html = $"<p>{sellerMessage}</p><p>Order ID: {order.OrderId}</p>";
                    await _emailService.SendAsync(sellerUser.Email, sellerTitle, html);
                }
                catch
                {
                    // Swallow email exceptions — notification already saved
                }
            }

            // --- Buyer notification (skip if buyer == seller) ---
            if (order.BuyerId != order.SellerId)
            {
                var buyerTitle = $"Order placed #{order.OrderId}";
                var buyerMessage = $"Your order (#{order.OrderId}) has been placed and sent to the seller {sellerUser?.FullName ?? "the seller"}.";

                var buyerNotification = new Notification
                {
                    UserId = order.BuyerId,
                    OrderId = order.OrderId,
                    Title = buyerTitle,
                    Message = buyerMessage,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                var savedBuyerNotification = await _notificationRepo.CreateAsync(buyerNotification);

                // Real-time push to connected buyer
                try
                {
                    var payloadBuyer = new
                    {
                        id = savedBuyerNotification.NotificationId,
                        title = savedBuyerNotification.Title,
                        message = savedBuyerNotification.Message,
                        orderId = savedBuyerNotification.OrderId,
                        createdAt = savedBuyerNotification.CreatedAt,
                        isRead = savedBuyerNotification.IsRead
                    };

                    await _hubContext.Clients.User(savedBuyerNotification.UserId.ToString()).SendAsync("ReceiveNotification", payloadBuyer);
                }
                catch
                {
                    // Ignore hub errors
                }

                // Optional: send email to buyer
                if (!string.IsNullOrWhiteSpace(buyerUser?.Email))
                {
                    try
                    {
                        var html = $"<p>{buyerMessage}</p><p>Order ID: {order.OrderId}</p>";
                        await _emailService.SendAsync(buyerUser.Email, buyerTitle, html);
                    }
                    catch
                    {
                        // Swallow email exceptions
                    }
                }
            }
        }

        public Task<List<Notification>> GetUserNotificationsAsync(long userId)
        {
            return _notificationRepo.GetByUserIdAsync(userId);
        }

        public Task MarkAsReadAsync(long notificationId)
        {
            return _notificationRepo.MarkAsReadAsync(notificationId);
        }
    }
}