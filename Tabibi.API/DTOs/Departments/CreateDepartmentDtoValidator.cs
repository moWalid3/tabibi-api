using FluentValidation;

namespace Tabibi.API.DTOs.Departments
{
    public sealed class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
    {
        public CreateDepartmentDtoValidator()
        {
            RuleFor(d => d.Name)
                .NotEmpty()
                .WithMessage("Name must not be empty")
                .MaximumLength(200)
                .WithMessage("Name can not exceed 200 characters");

            RuleFor(d => d.Description)
                .MaximumLength(1000)
                .WithMessage("Description can not exceed 1000 characters");

            RuleFor(d => d.ImageUrl)
                .MaximumLength(300)
                .WithMessage("Image url can not exceed 300 characters")
                .Matches(@"^(?:(?:https?|ftp):\/\/)?(?:www\.)?[a-z0-9-]+(?:\.[a-z0-9-]+)+[^\s]*$")
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("Image URL is not valid.");
        }
    }
}
