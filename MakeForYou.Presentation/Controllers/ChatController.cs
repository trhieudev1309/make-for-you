using MakeForYou.BusinessLogic.Services.Implement;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MakeForYou.Presentation.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IOrderService _orderService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, IOrderService orderService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _orderService = orderService;
            _logger = logger;
        }

        private long GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(userIdStr, out var userId))
                return userId;
            throw new UnauthorizedAccessException("User ID not found");
        }

        /// <summary>
        /// Get all unique conversation partners for current user plus latest message preview
        /// </summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = GetUserId();
                var users = await _chatService.GetConversationsAsync(userId);

                var list = new List<object>();
                foreach (var u in users)
                {
                    var msgs = await _chatService.GetMessagesAsync(userId, u.UserId);
                    var last = msgs.LastOrDefault();

                    // Query orderId từ bảng Orders theo cặp buyer/seller
                    var orderId = await _orderService.GetOrderIdByUsersAsync(userId, u.UserId);

                    list.Add(new
                    {
                        userId = u.UserId,
                        fullName = u.FullName ?? "Unknown",
                        lastMessage = last?.Message,
                        lastFromUserId = last?.FromUserId,
                        lastMessageAt = last?.CreatedAt,
                        orderId = orderId
                    });
                }

                _logger.LogInformation("User {UserId} fetched {Count} conversations", userId, list.Count);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversations for current user");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] long otherUserId)
        {
            try
            {
                var userId = GetUserId();
                var messages = await _chatService.GetMessagesAsync(userId, otherUserId);
                var result = messages.Select(m => new
                {
                    fromUserId = m.FromUserId,
                    toUserId = m.ToUserId,
                    fromUserName = m.FromUser?.FullName ?? "Unknown",
                    message = m.Message,
                    createdAt = m.CreatedAt,
                    isRead = m.IsRead
                }).ToList();

                _logger.LogInformation("User {UserId} fetched {Count} messages with user {OtherUserId}", userId, result.Count, otherUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for current user with user {OtherUserId}", otherUserId);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _chatService.GetUnreadCountAsync(userId);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread message count for current user");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            try
            {
                var userId = GetUserId();
                await _chatService.MarkAsReadAsync(request.FromUserId, request.ToUserId, userId);
                _logger.LogInformation("User {UserId} marked messages from {FromUserId} as read", userId, request.FromUserId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class MarkAsReadRequest
    {
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
    }
}
