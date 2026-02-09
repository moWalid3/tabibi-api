using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Notifications
{
    public sealed record SendNotificationDto(
        Guid Id,
        string Title,
        string Message,
        NotificationType Type,
        DateTime CreatedAt,
        Guid? RelatedEntityId
    );
}
