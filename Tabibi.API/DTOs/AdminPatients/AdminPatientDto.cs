using Tabibi.API.DTOs.Cities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.AdminPatients
{
    public sealed record AdminPatientDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public string? AvatarUrl { get; init; }
        public Gender? Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public string? City { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
