using System.Security.Claims;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MakeForYou.BusinessLogic.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private const string GroupPrefix = "chat_";

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        private string? GetUserId() =>
            Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private static string GetGroupName(string a, string b)
        {
            var ordered = new[] { a, b }.OrderBy(x => x).ToArray();
            return $"{GroupPrefix}{ordered[0]}_{ordered[1]}";
        }

        // Called by the client after connection to join the chat group
        public Task JoinChat(string artisanId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(artisanId))
                return Task.CompletedTask;

            var group = GetGroupName(userId, artisanId);
            return Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        // Optional: allow leaving the chat on disconnect / client request
        public Task LeaveChat(string artisanId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(artisanId))
                return Task.CompletedTask;

            var group = GetGroupName(userId, artisanId);
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }

        // client calls SendMessage(artisanId, message)
        public async Task SendMessage(string artisanId, string message)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(artisanId) || string.IsNullOrWhiteSpace(message))
                return;

            if (!long.TryParse(userId, out var userIdLong) || !long.TryParse(artisanId, out var artisanIdLong))
                return;

            // Save to database
            var savedMsg = await _chatRepository.AddMessageAsync(userIdLong, artisanIdLong, message.Trim());

            var group = GetGroupName(userId, artisanId);
            var fromUserName = Context.User?.Identity?.Name ?? userId;

            // Broadcast to group
            await Clients.Group(group).SendAsync("ReceiveMessage", new
            {
                fromUserId = userId,
                toUserId = artisanId,
                fromUserName,
                message = message.Trim(),
                sentAt = savedMsg.CreatedAt.ToString("o")
            });
        }
    }
}
