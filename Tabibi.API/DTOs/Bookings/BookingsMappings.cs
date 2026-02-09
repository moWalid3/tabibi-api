using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Bookings
{
    public static class BookingsMappings
    {
        public static PatientBookingDto ToPatientBookingDto(this Booking booking)
        {
            return new PatientBookingDto
            {
                Id = booking.Id,
                DoctorId = booking.Doctor?.Id,
                DoctorName = booking.Doctor?.Name,
                DoctorAvatar = booking.Doctor?.AvatarUrl,
                Department = booking.Doctor.Department?.Name,
                Address = booking.Doctor.Clinic?.Address,
                AppointmentDate = booking.AppointmentDate,
                Status = booking.Status,
                Type = booking.Type,
                ShowReviewButton = (booking.Status == BookingStatus.Completed && booking.Review == null),
                ShowPrescriptionButton = (booking.Status == BookingStatus.Completed && booking.Prescription != null)
            };
        }

        public static PrescriptionDetailsDto ToPrescriptionDetailsDto(this Prescription prescription)
        {
            return new PrescriptionDetailsDto
            (
                prescription.Id,
                prescription.Diagnosis,
                prescription.Notes,
                prescription.CreatedAt,
                prescription.Medicines?.Select(m => new MedicineItemDto(
                    m.MedicineName,
                    m.Dosage,
                    m.Frequency,
                    m.Duration,
                    m.Instructions
                )).ToList() ?? []
            );
        }

        public static Booking ToEntity(
            this CreateBookingDto dto,
            string patientId,
            decimal pricePaid,
            string? paymentIntentId)
        {
            return new Booking
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                Type = dto.Type,
                Status = BookingStatus.AwaitingPayment,
                PricePaid = pricePaid,
                PaymentIntentId = paymentIntentId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
