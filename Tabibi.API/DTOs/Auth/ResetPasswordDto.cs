using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Auth
{
    [ValidateNever]
    public sealed record ResetPasswordDto(string Email, string Password, string Token);
}
