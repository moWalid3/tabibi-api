using FluentValidation;

namespace Tabibi.API.DTOs.Doctors
{
    public class ClinicDetailsDtoValidator : AbstractValidator<ClinicDetailsDto>
    {
        public ClinicDetailsDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Clinic name is required.")
                .MaximumLength(200)
                .WithMessage("Clinic name cannot exceed 200 characters.");

            RuleFor(c => c.Description)
                .MaximumLength(1000)
                .WithMessage("Clinic description cannot exceed 1000 characters.");

            RuleFor(c => c.Address)
                .NotEmpty()
                .WithMessage("Clinic address is required.")
                .MaximumLength(300)
                .WithMessage("Address cannot exceed 300 characters.");

            RuleFor(p => p.ImageUrl)
                .MaximumLength(300)
                .WithMessage("Clinic image URL can not exceed 300 characters")
                .Matches(@"^(?:(?:https?|ftp):\/\/)?(?:www\.)?[a-z0-9-]+(?:\.[a-z0-9-]+)+[^\s]*$")
                .When(p => !string.IsNullOrWhiteSpace(p.ImageUrl))
                .WithMessage("Clinic image URL is not valid.");

            RuleFor(c => c.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^01[0125]\d{8}$")
                .WithMessage("Invalid phone number. It must be a valid Egyptian mobile number (e.g., 010xxxxxxxx).");

            RuleFor(c => c.CityId)
                .NotEmpty()
                .WithMessage("City is required.");
        }
    }
}