namespace MakeForYou.BusinessLogic.Entities
{
    public class ChatMessage
    {
        public long MessageId { get; set; }

        /// <summary>
        /// User sending the message
        /// </summary>
        public long FromUserId { get; set; }
        public User? FromUser { get; set; }

        /// <summary>
        /// User receiving the message
        /// </summary>
        public long ToUserId { get; set; }
        public User? ToUser { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}