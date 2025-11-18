using Microsoft.AspNetCore.Mvc;

namespace Tabibi.API.DTOs.Departments
{
    public sealed record DepartmentQueryParameters
    {
        [FromQuery(Name = "q")]
        public string? Search { get; set; }
        public string? Sort { get; init; }
    }
}
