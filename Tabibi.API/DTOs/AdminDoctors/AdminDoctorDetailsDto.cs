using Tabibi.API.DTOs.Departments;
using Tabibi.API.DTOs.Doctors;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.AdminDoctors
{
    public sealed record AdminDoctorDetailsDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required string Status { get; init; }
        public DoctorStatus StatusCode { get; init; }
        public string? AvatarUrl { get; init; }
        public Gender? Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public string? Bio { get; init; }
        public string? CredentialImageUrl { get; init; }
        public decimal ConsultationFee { get; init; }
        public int YearsOfExperience { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public DepartmentBasicDto? Department { get; init; }
        public AdminDoctorClinicDto? Clinic { get; init; }
        public List<AdminDoctorWorkScheduleDto>? Schedule { get; init; }
    }
}
