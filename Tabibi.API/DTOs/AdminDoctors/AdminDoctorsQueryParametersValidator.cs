using FluentValidation;

namespace Tabibi.API.DTOs.AdminDoctors
{
    public sealed class AdminDoctorsQueryParametersValidator : AbstractValidator<AdminDoctorsQueryParameters>
    {
        public AdminDoctorsQueryParametersValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 1000)
                .WithMessage("Page size must be between 1 and 1000");

            RuleFor(p => p.Gender)
                .IsInEnum()
                .When(p => p.Gender != null)
                .WithMessage("Gender is not valid");

            RuleFor(p => p.Status)
                .IsInEnum()
                .When(p => p.Status != null)
                .WithMessage("Status is not valid");
        }
    }
}
