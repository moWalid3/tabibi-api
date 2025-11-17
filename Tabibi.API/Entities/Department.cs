namespace Tabibi.API.Entities
{
    public sealed class Department
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public List<Doctor> Doctors { get; } = [];
    }
}
