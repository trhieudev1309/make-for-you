using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Hubs;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IUserRepository _userRepo;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(
            IChatRepository chatRepo,
            IUserRepository userRepo,
            IHubContext<ChatHub> hubContext)
        {
            _chatRepo = chatRepo;
            _userRepo = userRepo;
            _hubContext = hubContext;
        }

        public Task<List<User>> GetConversationsAsync(long userId) =>
            _chatRepo.GetConversationsAsync(userId);

        public Task<List<ChatMessage>> GetMessagesAsync(long userId, long otherUserId) =>
            _chatRepo.GetMessagesAsync(userId, otherUserId);

        public async Task<ChatMessage> SendMessageAsync(long fromUserId, long toUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty.", nameof(message));

            // Persist message
            var saved = await _chatRepo.AddMessageAsync(fromUserId, toUserId, message.Trim());

            // Resolve sender name for payload (best-effort)
            var sender = await _userRepo.GetByIdAsync(fromUserId);
            var fromUserName = sender?.FullName ?? sender?.Email ?? fromUserId.ToString();

            // Build payload delivered to clients
            var payload = new
            {
                fromUserId = fromUserId.ToString(),
                toUserId = toUserId.ToString(),
                fromUserName,
                message = saved.Message,
                sentAt = saved.CreatedAt.ToString("o")
            };

            // Broadcast to both participants by user id (SignalR User identifier typically NameIdentifier claim)
            try
            {
                var userStrings = new[] { fromUserId.ToString(), toUserId.ToString() };
                await _hubContext.Clients.Users(userStrings).SendAsync("ReceiveMessage", payload);
            }
            catch
            {
                // Do not fail the operation if hub broadcast fails; message is already saved.
            }

            return saved;
        }

        public Task MarkAsReadAsync(long fromUserId, long toUserId, long readByUserId) =>
            _chatRepo.MarkAsReadAsync(fromUserId, toUserId, readByUserId);

        public Task<int> GetUnreadCountAsync(long userId) =>
            _chatRepo.GetUnreadCountAsync(userId);
    }
}
