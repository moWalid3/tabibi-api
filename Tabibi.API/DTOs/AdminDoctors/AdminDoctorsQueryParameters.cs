using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.AdminDoctors
{
    [ValidateNever]
    public sealed record AdminDoctorsQueryParameters
    {
        [FromQuery(Name = "q")]
        public string? Search { get; set; }
        public Gender? Gender { get; init; }
        public bool? EmailConfirmed { get; init; }
        public DoctorStatus? Status { get; set; }
        public string? CityId { get; init; }
        public string? DepartmentId { get; init; }
        public string? Sort { get; init; } = "CreatedAtUtc desc";
        public string? Fields { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
