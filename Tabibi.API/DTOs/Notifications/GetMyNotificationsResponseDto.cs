using Tabibi.API.Common;

namespace Tabibi.API.DTOs.Notifications
{
    public sealed record GetMyNotificationsResponseDto : PaginationResult<NotificationDto>
    {
        public required int UnreadCount { get; init; }
    }
}
