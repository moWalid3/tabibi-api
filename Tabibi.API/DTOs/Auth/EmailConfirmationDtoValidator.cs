using FluentValidation;

namespace Tabibi.API.DTOs.Auth
{
    public sealed class EmailConfirmationDtoValidator : AbstractValidator<EmailConfirmationDto>
    {
        public EmailConfirmationDtoValidator()
        {
            RuleFor(ec => ec.Email)
                .NotEmpty()
                .WithMessage("Email must not be empty")
                .MaximumLength(300)
                .WithMessage("Email can not exceed 300 characters")
                .EmailAddress()
                .WithMessage("Must be a valid email address");

            RuleFor(ec => ec.Code)
                .NotEmpty()
                .WithMessage("Code must not be empty")
                .Length(6)
                .WithMessage("Code length must be 6-digits");
        }
    }
}
