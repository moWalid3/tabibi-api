using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Patients
{
    public sealed record UpdatePatientProfileDto
    {
        public required string Name { get; init; }
        public string? AvatarUrl { get; init; }
        public Gender? Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public string? CityId { get; init; }
    }
}
