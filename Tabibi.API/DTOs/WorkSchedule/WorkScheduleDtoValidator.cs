using FluentValidation;

namespace Tabibi.API.DTOs.WorkSchedule
{
    public class WorkScheduleDtoValidator : AbstractValidator<WorkScheduleDto>
    {
        public WorkScheduleDtoValidator()
        {
            RuleFor(ws => ws.DayOfWeek)
                .IsInEnum()
                .WithMessage("Invalid day of week.");

            // Ensure CloseTime is strictly after OpenTime when times are actually provided
            RuleFor(ws => ws)
                .Must(ws => ws.CloseTime > ws.OpenTime)
                .WithMessage("Close time must be after open time.")
                .When(ws => ws.OpenTime != default && ws.CloseTime != default);
        }
    }
}