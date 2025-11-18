using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Departments
{
    [ValidateNever]
    public sealed record CreateDepartmentDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
    }
}
