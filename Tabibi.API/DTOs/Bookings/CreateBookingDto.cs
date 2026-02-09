using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Bookings
{
    public sealed record CreateBookingDto(
        string DoctorId,
        DateTime AppointmentDate, // 01/27/2026 20:30:00
        AppointmentType Type);
}
