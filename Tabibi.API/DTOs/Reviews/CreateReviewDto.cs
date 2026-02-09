namespace Tabibi.API.DTOs.Reviews
{
    public sealed record CreateReviewDto(Guid BookingId, int Rating, string? Comment);
}
