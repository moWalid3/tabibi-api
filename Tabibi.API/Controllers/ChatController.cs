using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Chat;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;
using Tabibi.API.Hubs;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Patient}")]
    [Route("chat")]
    [ApiController]
    public sealed class ChatController(
        AppDbContext dbContext,
        IHubContext<MainHub> hubContext) : ControllerBase
    {
        [HttpGet("messages/{otherUserId}")]
        [ProducesResponseType<ChatHistoryResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMessages(string otherUserId)
        {
            string? myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<ChatMessageDto> messages = await dbContext.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == myId))
                .OrderBy(m => m.SentAt)
                .Select(m => m.ToDto(isMe: m.SenderId == myId))
                .ToListAsync();

            List<ChatMessage> unreadMessages = await dbContext.ChatMessages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == myId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Count != 0)
            {
                foreach (ChatMessage msg in unreadMessages)
                {
                    msg.IsRead = true;
                }

                await dbContext.SaveChangesAsync();
            }

            DateTime cutoffDate = DateTime.UtcNow.AddDays(-7);

            bool hasValidBooking = await dbContext.Bookings.AnyAsync(b =>
                ((b.PatientId == myId && b.DoctorId == otherUserId) ||
                 (b.PatientId == otherUserId && b.DoctorId == myId))
                &&
                (
                    b.Status == BookingStatus.Confirmed ||
                    (b.Status == BookingStatus.Completed && b.AppointmentDate > cutoffDate)
                ));

            ChatHistoryResponseDto response = new(
                CanChat: hasValidBooking,
                Messages: messages
            );

            return Ok(response);
        }


        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMessage(SendMessageRequestDto request)
        {
            string? myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DateTime cutoffDate = DateTime.UtcNow.AddDays(-7);

            bool canChat = await dbContext.Bookings.AnyAsync(b =>
                ((b.PatientId == myId && b.DoctorId == request.ReceiverId) ||
                 (b.PatientId == request.ReceiverId && b.DoctorId == myId))
                &&
                (
                    b.Status == BookingStatus.Confirmed ||
                    (b.Status == BookingStatus.Completed && b.AppointmentDate > cutoffDate)
                ));

            if (!canChat)
            {
                return BadRequest("Chat is not allowed. You need an active booking or a recently completed one (7 days).");
            }

            ChatMessage message = request.ToEntity(senderId: myId!);

            await dbContext.ChatMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();

            SendChatMessageDto result = new(message.Id, myId!, message.Message, message.SentAt);

            await hubContext.Clients.Group(request.ReceiverId)
                .SendAsync("ReceiveChatMessage", result);

            return Ok();
        }


        [HttpGet("conversations")]
        [ProducesResponseType<List<ChatConversationDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConversations()
        {
            string? myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversationData = await dbContext.ChatMessages
                .Where(m => m.SenderId == myId || m.ReceiverId == myId)
                .GroupBy(m => m.SenderId == myId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.SentAt)
                                   .Select(m => m.Message)
                                   .FirstOrDefault(),
                    LastMessageTime = g.Max(m => m.SentAt),
                    UnreadCount = g.Count(m => m.ReceiverId == myId && !m.IsRead)
                })
                .OrderByDescending(x => x.LastMessageTime)
                .ToListAsync();

            if (conversationData.Count == 0)
            {
                return Ok(new List<ChatConversationDto>());
            }

            List<string> userIds = conversationData.Select(c => c.OtherUserId).ToList();

            var users = await dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.AvatarUrl
                })
                .ToDictionaryAsync(u => u.Id);

            var result = conversationData.Select(c =>
            {
                bool userExists = users.TryGetValue(c.OtherUserId, out var user);

                return new ChatConversationDto(
                    c.OtherUserId,
                    userExists ? user!.Name : "Unknown User",
                    userExists ? user!.AvatarUrl : null,
                    c.LastMessage ?? "",
                    c.LastMessageTime,
                    c.UnreadCount
                );
            }).ToList();

            return Ok(result);
        }
    }
}
