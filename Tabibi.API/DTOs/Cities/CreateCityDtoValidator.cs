using FluentValidation;

namespace Tabibi.API.DTOs.Cities
{
    public sealed class CreateCityDtoValidator : AbstractValidator<CreateCityDto>
    {
        public CreateCityDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage("Name must not be empty")
                .MaximumLength(100)
                .WithMessage("Name can not exceed 100 characters");
        }
    }
}
