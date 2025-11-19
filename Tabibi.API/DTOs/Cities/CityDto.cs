namespace Tabibi.API.DTOs.Cities
{
    public sealed record CityDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
    }
}
