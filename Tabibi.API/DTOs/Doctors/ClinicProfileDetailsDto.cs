using Tabibi.API.DTOs.Cities;

namespace Tabibi.API.DTOs.Doctors
{
    public sealed record ClinicProfileDetailsDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required string Address { get; init; }
        public string? ImageUrl { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public required string PhoneNumber { get; init; }
        public required CityDto City { get; init; }
    }
}
