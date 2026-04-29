using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tabibi.API.DTOs.WorkSchedule;

namespace Tabibi.API.DTOs.Doctors
{
    [ValidateNever]
    public sealed record UpdateDoctorScheduleDto
    {
        public required List<WorkScheduleDto> Schedule { get; init; }
    }
}
