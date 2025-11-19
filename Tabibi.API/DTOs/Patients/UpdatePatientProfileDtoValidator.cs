using FluentValidation;

namespace Tabibi.API.DTOs.Patients
{
    public sealed class UpdatePatientProfileDtoValidator : AbstractValidator<UpdatePatientProfileDto>
    {
        public UpdatePatientProfileDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .WithMessage("Name must not be empty")
                .MaximumLength(300)
                .WithMessage("Name can not exceed 300 characters");

            RuleFor(p => p.AvatarUrl)
                .MaximumLength(500)
                .WithMessage("Avatar URL can not exceed 500 characters")
                .Matches(@"^(?:(?:https?|ftp):\/\/)?(?:www\.)?[a-z0-9-]+(?:\.[a-z0-9-]+)+[^\s]*$")
                .When(p => !string.IsNullOrWhiteSpace(p.AvatarUrl))
                .WithMessage("Avatar URL is not valid.");

            RuleFor(p => p.Gender)
                .IsInEnum()
                .When(p => p.Gender != null)
                .WithMessage("Gender is not valid");
        }
    }
}
