using FluentValidation;

namespace Tabibi.API.DTOs.Auth
{
    public sealed class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(fb => fb.Email)
                .NotEmpty()
                .WithMessage("Email must not be empty")
                .MaximumLength(300)
                .WithMessage("Email can not exceed 300 characters")
                .EmailAddress()
                .WithMessage("Must be a valid email address");
        }
    }
}
