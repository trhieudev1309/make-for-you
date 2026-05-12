using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface IChatRepository
    {
        /// <summary>
        /// Get all conversations for a user (unique list of users they've chatted with)
        /// </summary>
        Task<List<User>> GetConversationsAsync(long userId);

        /// <summary>
        /// Get all messages between two users
        /// </summary>
        Task<List<ChatMessage>> GetMessagesAsync(long userId1, long userId2);

        /// <summary>
        /// Save a new chat message
        /// </summary>
        Task<ChatMessage> AddMessageAsync(long fromUserId, long toUserId, string message);

        /// <summary>
        /// Mark messages as read
        /// </summary>
        Task MarkAsReadAsync(long fromUserId, long toUserId, long readByUserId);

        /// <summary>
        /// Get unread count for a user
        /// </summary>
        Task<int> GetUnreadCountAsync(long userId);
    }
}