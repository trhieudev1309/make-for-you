using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// Get unique conversation partners for a user.
        /// </summary>
        Task<List<User>> GetConversationsAsync(long userId);

        /// <summary>
        /// Get all messages between two users (chronological).
        /// </summary>
        Task<List<ChatMessage>> GetMessagesAsync(long userId, long otherUserId);

        /// <summary>
        /// Send a message: persist and broadcast in real-time.
        /// Returns the persisted ChatMessage.
        /// </summary>
        Task<ChatMessage> SendMessageAsync(long fromUserId, long toUserId, string message);

        /// <summary>
        /// Mark messages sent from fromUserId to toUserId as read by readByUserId.
        /// </summary>
        Task MarkAsReadAsync(long fromUserId, long toUserId, long readByUserId);

        /// <summary>
        /// Get unread messages count for a user.
        /// </summary>
        Task<int> GetUnreadCountAsync(long userId);
    }
}
