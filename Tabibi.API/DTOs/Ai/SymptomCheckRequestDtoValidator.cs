using FluentValidation;

namespace Tabibi.API.DTOs.Ai
{
    public class SymptomCheckRequestDtoValidator : AbstractValidator<SymptomCheckRequestDto>
    {
        public SymptomCheckRequestDtoValidator()
        {
            RuleFor(x => x.SymptomsText)
                .NotEmpty()
                .WithMessage("Symptoms text cannot be empty.");
        }
    }
}
