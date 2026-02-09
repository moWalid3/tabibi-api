namespace Tabibi.API.DTOs.Reviews
{
    public sealed record ReviewStatsDto(
        double AverageRating,
        int TotalReviews,
        int FiveStarCount,
        int FourStarCount,
        int ThreeStarCount,
        int TwoStarCount,
        int OneStarCount
    );
}
