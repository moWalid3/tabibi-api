using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Doctors
{
    [ValidateNever]
    public sealed record WorkScheduleDetailsDto
    {
        public DayOfWeek DayOfWeek { get; init; }
        public TimeSpan OpenTime { get; init; } // 09:00
        public TimeSpan CloseTime { get; init; }
    }
}
