using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Doctors
{
    [ValidateNever]
    public sealed record DoctorsQueryParameters
    {
        [FromQuery(Name = "q")]
        public string? Search { get; set; }
        public Gender? Gender { get; init; }
        public string? CityId { get; init; }
        public string? DepartmentId { get; init; }
        public string? Sort { get; init; }
        public string? SortByRating { get; init; }
        public string? SortByReviewCount { get; init; }
        public string? Fields { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
