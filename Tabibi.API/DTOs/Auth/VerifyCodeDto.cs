namespace Tabibi.API.DTOs.Auth
{
    public sealed record VerifyCodeDto(string Email, string Code);
}
