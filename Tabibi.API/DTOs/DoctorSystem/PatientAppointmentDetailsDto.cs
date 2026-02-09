using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record PatientAppointmentDetailsDto(
        string Id,
        string Name,
        string Email,
        string? AvatarUrl,
        Gender? Gender,
        DateOnly? DateOfBirth,
        string? City
    );
}
