namespace Tabibi.API.Entities
{
    public class Patient : ApplicationUser
    {
        public string? CityId { get; set; }
        public City? City { get; set; }
    }
}
