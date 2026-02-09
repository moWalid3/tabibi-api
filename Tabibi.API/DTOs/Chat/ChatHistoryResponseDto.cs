namespace Tabibi.API.DTOs.Chat
{
    public sealed record ChatHistoryResponseDto(
        bool CanChat,
        List<ChatMessageDto> Messages
    );
}
