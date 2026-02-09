namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record DoctorHomeResponseDto(
        string DoctorName,
        string? DoctorAvatarUrl,
        DoctorHomeStatsDto Stats,
        List<DoctorAppointmentDto> TodayAppointments
    );
}
