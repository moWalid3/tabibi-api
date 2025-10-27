namespace Tabibi.API.DTOs.Auth
{
    public sealed record ResetPasswordDto(string Email, string Password, string Token);
}
