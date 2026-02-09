namespace Tabibi.API.DTOs.Chat
{
    public sealed record SendChatMessageDto(
        Guid Id,
        string SenderId,
        string Message,
        DateTime SentAt
    );
}
