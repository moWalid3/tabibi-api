namespace Tabibi.API.DTOs.Chat
{
    public sealed record SendMessageRequestDto(string ReceiverId, string Content);
}
