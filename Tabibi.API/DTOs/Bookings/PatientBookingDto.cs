using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Bookings
{
    public sealed record PatientBookingDto
    {
        public required Guid Id { get; init; }
        public required string DoctorId { get; init; }
        public required string DoctorName { get; init; }
        public string? DoctorAvatar { get; init; }
        public required string Department { get; init; }
        public required string Address { get; init; }
        public required DateTime AppointmentDate { get; init; }
        public required AppointmentType Type { get; init; }
        public required BookingStatus Status { get; init; }
        public bool ShowReviewButton { get; init; }
        public bool ShowPrescriptionButton { get; init; }
    }
}
