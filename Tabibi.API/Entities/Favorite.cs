namespace Tabibi.API.Entities
{
    public sealed class Favorite
    {
        public required string PatientId { get; set; }
        public required string DoctorId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
