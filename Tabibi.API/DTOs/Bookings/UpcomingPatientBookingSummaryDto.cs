namespace Tabibi.API.DTOs.Bookings
{
    public sealed record UpcomingPatientBookingSummaryDto(
        int TotalUpcomingCount,
        PatientBookingDto? NextBooking
    );
}
