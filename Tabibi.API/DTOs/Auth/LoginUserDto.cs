using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Auth
{
    [ValidateNever]
    public sealed record LoginUserDto
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
