using FluentValidation;

namespace Tabibi.API.DTOs.Auth
{
    public sealed class VerifyCodeDtoValidator : AbstractValidator<VerifyCodeDto>
    {
        public VerifyCodeDtoValidator()
        {
            RuleFor(vc => vc.Email)
                .NotEmpty()
                .WithMessage("Email must not be empty")
                .MaximumLength(300)
                .WithMessage("Email can not exceed 300 characters")
                .EmailAddress()
                .WithMessage("Must be a valid email address");

            RuleFor(vc => vc.Code)
                .NotEmpty()
                .WithMessage("Code must not be empty")
                .Length(6)
                .WithMessage("Code length must be 6-digits");
        }
    }
}
