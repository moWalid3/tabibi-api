using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Chat
{
    public static class ChatMappings
    {
        public static ChatMessageDto ToDto(this ChatMessage m, bool isMe)
        {
            return new ChatMessageDto
            (
                Id: m.Id,
                Message: m.Message,
                SentAt: m.SentAt,
                IsMe: isMe,
                IsRead: m.IsRead
            );
        }

        public static ChatMessage ToEntity(this SendMessageRequestDto request, string senderId)
        {
            return new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Message = request.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
        }
    }
}
