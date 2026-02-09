using Microsoft.AspNetCore.SignalR;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Notifications;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;
using Tabibi.API.Hubs;

namespace Tabibi.API.Services
{
    public sealed class NotificationService(AppDbContext context, IHubContext<MainHub> hubContext)
    {
        public async Task SendNotificationAsync(
            string receiverId,
            string title,
            string message,
            NotificationType type,
            Guid? relatedId = null)
        {
            Notification notification = new()
            {
                UserId = receiverId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RelatedEntityId = relatedId
            };

            await context.Notifications.AddAsync(notification);
            await context.SaveChangesAsync();

            SendNotificationDto result = new
                (notification.Id, title, message, type, notification.CreatedAt, relatedId);

            await hubContext.Clients.Group(receiverId)
                .SendAsync("ReceiveNotification", result);
        }
    }
}
