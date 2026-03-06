using FluentValidation;

namespace Tabibi.API.DTOs.Notifications
{
    public class UpdateFcmTokenDtoValidator : AbstractValidator<UpdateFcmTokenDto>
    {
        public UpdateFcmTokenDtoValidator()
        {
            RuleFor(u => u.FcmToken)
                .NotEmpty()
                .WithMessage("FcmToken must not be empty");
        }
    }
}
