using FluentValidation;

namespace Tabibi.API.DTOs.Bookings
{
    public sealed class RescheduleBookingRequestDtoValidator : AbstractValidator<RescheduleBookingRequestDto>
    {
        public RescheduleBookingRequestDtoValidator()
        {
            RuleFor(x => x.NewDate)
                .GreaterThan(DateTime.UtcNow.AddMinutes(15))
                .WithMessage("New appointment time must be at least 15 minutes in the future.");
        }
    }
}
