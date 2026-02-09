using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Notifications;
using Tabibi.API.Entities;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Patient}")]
    [Route("notifications")]
    [ApiController]
    public sealed class NotificationsController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<GetMyNotificationsResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IOrderedQueryable<Notification> query = dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            int totalCount = await query.CountAsync();

            List<NotificationDto> notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => n.ToDto())
                .ToListAsync();

            int unreadCount = await dbContext.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            GetMyNotificationsResponseDto result = new()
            {
                Items = notifications,
                UnreadCount = unreadCount,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }


        [HttpPatch("{id}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Notification? notification = await dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await dbContext.SaveChangesAsync();
            }

            return Ok();
        }


        [HttpPatch("read-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Notification> unreadNotifications = await dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Count != 0)
            {
                foreach (Notification n in unreadNotifications)
                {
                    n.IsRead = true;
                }

                await dbContext.SaveChangesAsync();
            }

            return Ok();
        }


        [HttpGet("unread-count")]
        [ProducesResponseType<GetUnreadCountResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            int count = await dbContext.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new GetUnreadCountResponseDto(count));
        }
    }
}
