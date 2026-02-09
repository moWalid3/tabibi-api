namespace Tabibi.API.Entities
{
    public sealed class ChatMessage
    {
        public Guid Id { get; set; }
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
