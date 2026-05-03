using FluentValidation;

namespace Tabibi.API.DTOs.MedicalProfile
{
    public sealed class UpdateMedicalProfileDtoValidator : AbstractValidator<UpdateMedicalProfileDto>
    {
        public UpdateMedicalProfileDtoValidator()
        {
            RuleFor(x => x.Weight)
                .InclusiveBetween(10, 300)
                .When(x => x.Weight.HasValue)
                .WithMessage("Weight must be between 10 and 300 kg.");

            RuleFor(x => x.Height)
                .InclusiveBetween(50, 250)
                .When(x => x.Height.HasValue)
                .WithMessage("Height must be between 50 and 250 cm.");

            RuleFor(x => x.Medications)
                .MaximumLength(1000)
                .WithMessage("Medications text is too long.");

            RuleFor(x => x.Allergies)
                .MaximumLength(1000)
                .WithMessage("Allergies text is too long.");
        }
    }
}
