using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Auth
{
    [ValidateNever]
    public sealed record VerifyCodeDto(string Email, string Code);
}
