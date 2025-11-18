namespace Tabibi.API.Entities
{
    public sealed class Patient : ApplicationUser
    {
        public string? CityId { get; set; }
        public City? City { get; set; }
    }
}
