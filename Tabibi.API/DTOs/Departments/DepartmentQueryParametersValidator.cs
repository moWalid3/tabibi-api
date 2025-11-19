using FluentValidation;

namespace Tabibi.API.DTOs.Departments
{
    public sealed class DepartmentQueryParametersValidator : AbstractValidator<DepartmentQueryParameters>
    {
        public DepartmentQueryParametersValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 1000)
                .WithMessage("Page size must be between 1 and 1000");
        }
    }
}
