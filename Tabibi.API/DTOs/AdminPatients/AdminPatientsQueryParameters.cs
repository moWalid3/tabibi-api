using Microsoft.AspNetCore.Mvc;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.AdminPatients
{
    public sealed record AdminPatientsQueryParameters
    {
        [FromQuery(Name = "q")]
        public string? Search { get; set; }
        public Gender? Gender { get; init; }
        public bool? EmailConfirmed { get; init; }
        public string? Sort { get; init; } = "CreatedAtUtc desc";
        public string? Fields { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
