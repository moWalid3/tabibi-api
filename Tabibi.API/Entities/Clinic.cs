namespace Tabibi.API.Entities
{
    public sealed class Clinic
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public string? ImageUrl { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public required string PhoneNumber { get; set; }

        public Guid CityId { get; set; }
        public City? City { get; set; }

        public Doctor? Doctor { get; set; }

        public List<WorkSchedule> Schedule { get; } = [];
    }
}
