using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetConversationsAsync(long userId)
        {
            // Get all unique users that current user has chatted with
            // Split into two queries to avoid ternary operator translation issues

            // Users where current user is the sender
            var sentToUsers = await _context.ChatMessages
                .Where(m => m.FromUserId == userId)
                .Select(m => m.ToUserId)
                .Distinct()
                .ToListAsync();

            // Users where current user is the receiver
            var receivedFromUsers = await _context.ChatMessages
                .Where(m => m.ToUserId == userId)
                .Select(m => m.FromUserId)
                .Distinct()
                .ToListAsync();

            // Combine and get unique user IDs
            var allConversationUserIds = sentToUsers
                .Union(receivedFromUsers)
                .ToList();

            // Fetch the actual User objects
            var conversationUsers = await _context.Users
                .Where(u => allConversationUserIds.Contains(u.UserId))
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return conversationUsers;
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(long userId1, long userId2)
        {
            // Get all messages between two users, sorted by creation time
            return await _context.ChatMessages
                .Where(m => (m.FromUserId == userId1 && m.ToUserId == userId2) ||
                           (m.FromUserId == userId2 && m.ToUserId == userId1))
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(long fromUserId, long toUserId, string message)
        {
            var chatMessage = new ChatMessage
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            return chatMessage;
        }

        public async Task MarkAsReadAsync(long fromUserId, long toUserId, long readByUserId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.FromUserId == fromUserId && m.ToUserId == toUserId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in messages)
            {
                msg.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(long userId)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ToUserId == userId && !m.IsRead);
        }
    }
}