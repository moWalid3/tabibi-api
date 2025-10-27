using FluentValidation;

namespace Tabibi.API.DTOs.Auth
{
    public sealed class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(rt => rt.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token must not be empty");
        }
    }
}
