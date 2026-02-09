using Tabibi.API.Common;

namespace Tabibi.API.DTOs.Reviews
{
    public sealed record DoctorReviewsPageDto(
        ReviewStatsDto Stats,
        PaginationResult<ReviewDto> Reviews
    );
}
