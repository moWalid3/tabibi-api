namespace Tabibi.API.DTOs.Auth
{
    public sealed record EmailConfirmationDto(string Email, string Code);
}
