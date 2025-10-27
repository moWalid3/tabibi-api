using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Auth
{
    [ValidateNever]
    public sealed record RegisterUserDto
    {
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required RoleDto Role { get; init; }
    }
}
