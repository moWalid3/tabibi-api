using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.WorkSchedule
{
    [ValidateNever]
    public sealed record WorkScheduleDto
    {
        public DayOfWeek DayOfWeek { get; init; }
        public TimeSpan OpenTime { get; init; } // 09:00
        public TimeSpan CloseTime { get; init; }
    }
}
