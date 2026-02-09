namespace Tabibi.API.Entities
{
    public sealed class Review
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string DoctorId { get; set; }
        public required string PatientId { get; set; }
        public Guid BookingId { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Booking? Booking { get; set; }
    }
}
