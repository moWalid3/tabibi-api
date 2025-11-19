using FluentValidation;

namespace Tabibi.API.DTOs.Auth
{
    public sealed class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(u => u.Name)
                .NotEmpty()
                .WithMessage("Name must not be empty")
                .MaximumLength(300)
                .WithMessage("Name can not exceed 300 characters");

            RuleFor(u => u.Email)
                .NotEmpty()
                .WithMessage("Email must not be empty")
                .MaximumLength(300)
                .WithMessage("Email can not exceed 300 characters")
                .EmailAddress()
                .WithMessage("Must be a valid email address");

            RuleFor(u => u.Password)
                .NotEmpty()
                .WithMessage("Password must not be empty")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

            RuleFor(u => u.Role)
                .IsInEnum()
                .WithMessage("Invalid role");
        }
    }
}
