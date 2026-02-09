using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record DoctorAppointmentDto(
        Guid Id,
        string PatientName,
        string? PatientAvatarUrl,
        DateTime AppointmentDate,
        AppointmentType Type,
        BookingStatus Status
    );
}
