using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Entities
{
    public sealed class Booking
    {
        public Guid Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public BookingStatus Status { get; set; }
        public AppointmentType Type { get; set; }
        public decimal PricePaid { get; set; }
        public string? PaymentIntentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string PatientId { get; set; }
        public required string DoctorId { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Review? Review { get; set; }
        public Prescription? Prescription { get; set; }
    }
}
