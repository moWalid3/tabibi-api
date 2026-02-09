namespace Tabibi.API.Entities
{
    public sealed class PrescriptionItem
    {
        public Guid Id { get; set; }
        public required string MedicineName { get; set; }
        public required string Dosage { get; set; }
        public required string Frequency { get; set; }
        public required string Duration { get; set; }
        public required string Instructions { get; set; }
        public Guid PrescriptionId { get; set; }

        public Prescription? Prescription { get; set; }
    }
}
