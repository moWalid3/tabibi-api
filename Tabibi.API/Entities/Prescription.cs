namespace Tabibi.API.Entities
{
    public sealed class Prescription
    {
        public Guid Id { get; set; }
        public required string Diagnosis { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid BookingId { get; set; }

        public Booking? Booking { get; set; }
        public ICollection<PrescriptionItem>? Medicines { get; set; }
    }
}
