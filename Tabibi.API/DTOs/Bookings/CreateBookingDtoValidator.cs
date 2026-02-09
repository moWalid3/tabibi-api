using FluentValidation;

namespace Tabibi.API.DTOs.Bookings
{
    public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDtoValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty()
                .WithMessage("DoctorId is required");

            RuleFor(x => x.AppointmentDate)
                .GreaterThan(DateTime.UtcNow.AddMinutes(15))
                .WithMessage("Appointment must be booked at least 15 minutes in advance.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid booking type");
        }
    }
}
