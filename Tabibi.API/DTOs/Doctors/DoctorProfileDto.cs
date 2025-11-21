using Tabibi.API.DTOs.Departments;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Doctors
{
    public sealed record DoctorProfileDto
    {
        public required string Name { get; init; }
        public required string Email { get; init; }
        public string? AvatarUrl { get; init; }
        public Gender? Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public string? Bio { get; init; }
        public decimal ConsultationFee { get; init; }
        public int YearsOfExperience { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public required DepartmentBasicDto Department { get; init; }
        public required ClinicProfileDetailsDto Clinic { get; init; }
        public required List<WorkScheduleDetailsDto> Schedule { get; init; }
    }
}
