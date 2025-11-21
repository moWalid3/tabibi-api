namespace Tabibi.API.DTOs.AdminDoctors
{
    public sealed record AdminDoctorWorkScheduleDto
    {
        public DayOfWeek DayOfWeek { get; init; }
        public TimeSpan OpenTime { get; init; } // 09:00
        public TimeSpan CloseTime { get; init; }
    }
}
