namespace Tabibi.API.Entities
{
    public sealed class Clinic
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public required string PhoneNumber { get; set; }
        public required string OpeningHours { get; set; } //"Sat-Thu: 10:00 - 18:00"

        public required string CityId { get; set; }
        public City? City { get; set; }

        public Doctor? Doctor { get; set; }
    }
}
