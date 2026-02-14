namespace Tabibi.API.DTOs.Video
{
    public sealed record VideoCallResponse(
        string Token,
        string RoomId,
        string UserId,
        string UserName,
        long AppId
    );
}
