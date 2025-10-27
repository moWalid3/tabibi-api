using FluentValidation;

namespace Tabibi.API.DTOs.Auth
{
    public sealed class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(rp => rp.Email)
                .NotEmpty()
                .WithMessage("Email must not be empty")
                .MaximumLength(300)
                .WithMessage("Email can not exceed 300 characters")
                .EmailAddress()
                .WithMessage("Must be a valid email address");

            RuleFor(rp => rp.Password)
                .NotEmpty()
                .WithMessage("Password must not be empty")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

            RuleFor(rp => rp.Token)
                .NotEmpty()
                .WithMessage("Token must not be empty");
        }
    }
}
