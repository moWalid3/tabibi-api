using Tabibi.API.DTOs.Cities;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.DTOs.Doctors;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.AdminDoctors
{
    public sealed record AdminDoctorDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required string Status { get; init; }
        public string? AvatarUrl { get; init; }
        public Gender? Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public decimal ConsultationFee { get; init; }
        public int YearsOfExperience { get; init; }
        public string? City { get; init; }
        public string? Department { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
