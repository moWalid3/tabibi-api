using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Entities
{
    public sealed class Notification
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? RelatedEntityId { get; set; }
    }
}
