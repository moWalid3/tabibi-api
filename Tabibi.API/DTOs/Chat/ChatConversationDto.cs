namespace Tabibi.API.DTOs.Chat
{
    public sealed record ChatConversationDto(
        string OtherUserId,
        string OtherUserName,
        string? OtherUserImage,
        string LastMessage,
        DateTime LastMessageTime,
        int UnreadCount
    );
}
