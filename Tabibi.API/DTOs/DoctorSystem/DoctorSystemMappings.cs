using Tabibi.API.DTOs.Bookings;
using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.DoctorSystem
{
    public static class DoctorSystemMappings
    {
        public static DoctorAppointmentDto ToDoctorAppointmentDto(this Booking b)
        {
            return new DoctorAppointmentDto(
                b.Id,
                b.Patient.Name,
                b.Patient.AvatarUrl,
                b.AppointmentDate,
                b.Type,
                b.Status
            );
        }

        public static AppointmentDetailsDto ToAppointmentDetailsDto(this Booking b)
        {
            return new AppointmentDetailsDto(
                Id: b.Id,
                AppointmentDate: b.AppointmentDate,
                Type: b.Type,
                Status: b.Status,
                Patient: b.Patient.ToPatientAppointmentDetailsDto(),
                Prescription: b.Prescription?.ToPrescriptionDetailsDto()
            );
        }

        public static TransactionItemDto ToTransactionItemDto(this Booking b)
        {
            return new TransactionItemDto(
                b.Id,
                b.Patient.Name,
                b.Patient.AvatarUrl,
                b.AppointmentDate.ToString("MMM dd, yyyy"),
                b.PricePaid,
                b.Type
            );
        }

        public static PatientAppointmentDetailsDto ToPatientAppointmentDetailsDto(this Patient patient)
        {
            return new PatientAppointmentDetailsDto(
                patient.Id,
                patient.Name,
                patient.Email!,
                patient.AvatarUrl,
                patient.Gender,
                patient.DateOfBirth,
                patient.City?.Name
            );
        }

        public static Prescription ToEntity(this CreatePrescriptionDto dto, Guid bookingId)
        {
            return new Prescription
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                Diagnosis = dto.Diagnosis,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                Medicines = dto.Medicines?.Select(m => new PrescriptionItem
                {
                    MedicineName = m.MedicineName,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Duration = m.Duration,
                    Instructions = m.Instructions
                }).ToList()
            };
        }

        public static PrescriptionItem ToEntity(this MedicineItemDto dto, Guid prescriptionId)
        {
            return new PrescriptionItem
            {
                PrescriptionId = prescriptionId,
                MedicineName = dto.MedicineName,
                Dosage = dto.Dosage,
                Frequency = dto.Frequency,
                Duration = dto.Duration,
                Instructions = dto.Instructions
            };
        }
    }
}
