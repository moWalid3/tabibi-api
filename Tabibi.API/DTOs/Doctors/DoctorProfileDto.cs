using Tabibi.API.DTOs.Clinic;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.DTOs.WorkSchedule;
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
        public DepartmentBasicDto? Department { get; init; }
        public ClinicDto? Clinic { get; init; }
        public List<WorkScheduleDto>? Schedule { get; init; }
    }
}
