using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Patients
{
    public sealed record PatientProfileDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public string? AvatarUrl { get; init; }
        public Gender? Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public Guid? CityId { get; init; }
    }
}
