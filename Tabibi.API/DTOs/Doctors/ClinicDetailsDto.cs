using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Doctors
{
    [ValidateNever]
    public sealed record ClinicDetailsDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required string Address { get; init; }
        public string? ImageUrl { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public required string PhoneNumber { get; init; }
        public required string CityId { get; init; }
    }
}
