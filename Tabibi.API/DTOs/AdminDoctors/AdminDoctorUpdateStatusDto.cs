using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.AdminDoctors
{
    [ValidateNever]
    public sealed record AdminDoctorUpdateStatusDto(DoctorStatus Status);

    public class AdminDoctorUpdateStatusDtoValidator : AbstractValidator<AdminDoctorUpdateStatusDto>
    {
        public AdminDoctorUpdateStatusDtoValidator()
        {
            RuleFor(s => s.Status)
                .IsInEnum()
                .WithMessage("Invalid doctor status");
        }
    }
}
