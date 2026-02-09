using FluentValidation;
using Tabibi.API.DTOs.Bookings;

namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed class CreatePrescriptionDtoValidator : AbstractValidator<CreatePrescriptionDto>
    {
        public CreatePrescriptionDtoValidator()
        {
            RuleFor(x => x.Diagnosis)
                .NotEmpty()
                .WithMessage("Diagnosis is required.")
                .MaximumLength(500)
                .WithMessage("Diagnosis cannot exceed 500 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters.");

            RuleFor(x => x.Medicines)
                .Must(m => m.Count <= 20)
                .WithMessage("Cannot add more than 20 medicines.");

            RuleForEach(x => x.Medicines).SetValidator(new MedicineItemDtoValidator());
        }
    }

    public sealed class MedicineItemDtoValidator : AbstractValidator<MedicineItemDto>
    {
        public MedicineItemDtoValidator()
        {
            RuleFor(x => x.MedicineName)
                .NotEmpty()
                .WithMessage("Medicine name is required.")
                .MaximumLength(200)
                .WithMessage("Medicine name is too long.");

            RuleFor(x => x.Dosage)
                .NotEmpty()
                .WithMessage("Dosage is required (e.g. 500mg).")
                .MaximumLength(100)
                .WithMessage("Dosage cannot exceed 100 characters.");

            RuleFor(x => x.Frequency)
                .NotEmpty()
                .WithMessage("Frequency is required (e.g. Twice daily).")
                .MaximumLength(100)
                .WithMessage("Frequency cannot exceed 100 characters.");

            RuleFor(x => x.Duration)
                .NotEmpty()
                .WithMessage("Duration is required (e.g. 5 days).")
                .MaximumLength(100)
                .WithMessage("Duration cannot exceed 100 characters.");

            RuleFor(x => x.Instructions)
                .MaximumLength(100)
                .WithMessage("Instructions cannot exceed 100 characters.");
        }
    }
}
