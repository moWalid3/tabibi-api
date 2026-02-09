namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record DoctorHomeStatsDto(
        int TodayCount,
        int CompletedCount,
        int CancelledCount
    );
}
