namespace Tabibi.API.DTOs.Bookings
{
    public sealed record CreateBookingResponseDto(Guid BookingId, string ClientSecret);
}
