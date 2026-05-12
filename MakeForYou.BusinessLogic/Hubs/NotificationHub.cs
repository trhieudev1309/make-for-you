using Microsoft.AspNetCore.SignalR;

namespace MakeForYou.BusinessLogic.Hubs
{
    public class NotificationHub : Hub
    {
        // Intentionally minimal: we rely on Clients.User(userId.ToString()) from server-side.
    }
}