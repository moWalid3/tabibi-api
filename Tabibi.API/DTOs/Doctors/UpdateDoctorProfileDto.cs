using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Doctors
{
    [ValidateNever]
    public sealed record UpdateDoctorProfileDto
    {
        public required string Name { get; init; }
        public string? AvatarUrl { get; init; }
        public Gender Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public string? Bio { get; init; }
        public decimal ConsultationFee { get; init; }
        public string? CredentialImageUrl { get; init; }
        public int YearsOfExperience { get; init; }
        public required string DepartmentId { get; init; }
        public required ClinicDetailsDto Clinic { get; init; }
        public required List<WorkScheduleDetailsDto> Schedule { get; init; }
    }
}
