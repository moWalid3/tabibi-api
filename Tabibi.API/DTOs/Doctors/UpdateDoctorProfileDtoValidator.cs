using FluentValidation;
using Tabibi.API.DTOs.WorkSchedule;

namespace Tabibi.API.DTOs.Doctors
{
    public sealed class UpdateDoctorProfileDtoValidator : AbstractValidator<UpdateDoctorProfileDto>
    {
        public UpdateDoctorProfileDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .WithMessage("Name must not be empty")
                .MaximumLength(300)
                .WithMessage("Name can not exceed 300 characters");

            RuleFor(p => p.AvatarUrl)
                .MaximumLength(500)
                .WithMessage("Avatar URL can not exceed 500 characters")
                .Matches(@"^(?:(?:https?|ftp):\/\/)?(?:www\.)?[a-z0-9-]+(?:\.[a-z0-9-]+)+[^\s]*$")
                .When(p => !string.IsNullOrWhiteSpace(p.AvatarUrl))
                .WithMessage("Avatar URL is not valid.");

            RuleFor(p => p.Gender)
                .IsInEnum()
                .WithMessage("Gender is not valid");

            RuleFor(p => p.DateOfBirth)
                .LessThan(DateOnly.FromDateTime(DateTime.Today))
                .When(p => p.DateOfBirth.HasValue)
                .WithMessage("Date of birth must be in the past.");

            RuleFor(p => p.Bio)
                .MaximumLength(1000)
                .WithMessage("Bio cannot exceed 1000 characters.");

            RuleFor(p => p.ConsultationFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Consultation fee cannot be negative.")
                .PrecisionScale(18, 2, false)
                .WithMessage("Consultation fee amount is invalid. It can not have more than 2 decimal places(e.g., 150.50) and must be less than 18 digits in total.");

            RuleFor(p => p.CredentialImageUrl)
                .NotEmpty()
                .WithMessage("Credential Image URL is required")
                .MaximumLength(300)
                .WithMessage("Credential image URL can not exceed 300 characters")
                .Matches(@"^(?:(?:https?|ftp):\/\/)?(?:www\.)?[a-z0-9-]+(?:\.[a-z0-9-]+)+[^\s]*$")
                .WithMessage("Credential image URL is not valid.");

            RuleFor(p => p.YearsOfExperience)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Years of experience cannot be negative.");

            RuleFor(p => p.DepartmentId)
                .NotEmpty()
                .WithMessage("Department is required.");

            // Validate the nested Clinic object
            RuleFor(p => p.Clinic)
                .NotNull()
                .WithMessage("Clinic details are required.")
                .SetValidator(new ClinicDetailsDtoValidator());

            // Validate the nested Schedule list
            RuleFor(p => p.Schedule)
                .NotEmpty()
                .WithMessage("At least one work schedule entry is required.")
                .Must(HaveUniqueCodes)
                .WithMessage("Each work schedule must have a unique DayOfWeek. Do not repeat DayOfWeek")
                .ForEach(schedule => schedule.SetValidator(new WorkScheduleDtoValidator()));
        }

        private bool HaveUniqueCodes(List<WorkScheduleDto> items)
        {
            return items
                .GroupBy(i => i.DayOfWeek)
                .All(g => g.Count() == 1);
        }
    }
}