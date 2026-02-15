namespace Tabibi.API.Entities
{
    public sealed class Patient : ApplicationUser
    {
        public Guid? CityId { get; set; }

        public City? City { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
    }
}
