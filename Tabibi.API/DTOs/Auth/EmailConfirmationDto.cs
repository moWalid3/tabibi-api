using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Auth
{
    [ValidateNever]
    public sealed record EmailConfirmationDto(string Email, string Code);
}
