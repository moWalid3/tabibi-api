namespace Tabibi.API.Entities
{
    public sealed class WorkSchedule
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; } // 09:00
        public TimeSpan CloseTime { get; set; }

        public required string ClinicId { get; set; }
        public Clinic? Clinic { get; set; }
    }
}
