using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendOrderNotificationAsync(Order order);
        Task<List<Notification>> GetUserNotificationsAsync(long userId);
        Task MarkAsReadAsync(long notificationId);
    }
}