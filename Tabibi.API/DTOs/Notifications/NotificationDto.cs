using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Notifications
{
    public sealed record NotificationDto(
        Guid Id,
        string Title,
        string Message,
        NotificationType Type,
        bool IsRead,
        DateTime CreatedAt,
        Guid? RelatedEntityId
    );
}
