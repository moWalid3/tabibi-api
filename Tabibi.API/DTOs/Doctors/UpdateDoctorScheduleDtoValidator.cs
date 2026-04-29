using FluentValidation;
using Tabibi.API.DTOs.WorkSchedule;

namespace Tabibi.API.DTOs.Doctors
{
    public sealed class UpdateDoctorScheduleDtoValidator : AbstractValidator<UpdateDoctorScheduleDto>
    {
        public UpdateDoctorScheduleDtoValidator()
        {
            RuleFor(p => p.Schedule)
                .NotEmpty()
                .WithMessage("At least one work schedule entry is required.")
                .Must(HaveUniqueCodes)
                .WithMessage("Each work schedule must have a unique DayOfWeek. Do not repeat DayOfWeek")
                .ForEach(schedule => schedule.SetValidator(new WorkScheduleDtoValidator()));
        }

        private bool HaveUniqueCodes(List<WorkScheduleDto> items)
        {
            return items
                .GroupBy(i => i.DayOfWeek)
                .All(g => g.Count() == 1);
        }
    }
}
