using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Notifications
{
    public static class NotificationsMappings
    {
        public static NotificationDto ToDto(this Notification n)
        {
            return new NotificationDto
            (
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt,
                n.RelatedEntityId
            );
        }
    }
}
