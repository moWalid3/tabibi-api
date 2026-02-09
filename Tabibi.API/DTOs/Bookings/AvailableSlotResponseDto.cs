namespace Tabibi.API.DTOs.Bookings
{
    public sealed record AvailableSlotResponseDto(DateTime StartTime, bool IsAvailable);
}
