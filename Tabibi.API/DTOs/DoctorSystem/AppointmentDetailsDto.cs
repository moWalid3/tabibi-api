using Tabibi.API.DTOs.Bookings;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record AppointmentDetailsDto(
        Guid Id,
        DateTime AppointmentDate,
        AppointmentType Type,
        BookingStatus Status,
        PatientAppointmentDetailsDto Patient,
        PrescriptionDetailsDto? Prescription
    );
}
