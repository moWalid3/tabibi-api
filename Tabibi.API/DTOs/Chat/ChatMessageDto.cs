namespace Tabibi.API.DTOs.Chat
{
    public sealed record ChatMessageDto(
        Guid Id,
        string Message,
        DateTime SentAt,
        bool IsMe,
        bool IsRead
    );
}
