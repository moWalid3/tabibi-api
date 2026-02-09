using Tabibi.API.DTOs.Bookings;

namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record CreatePrescriptionDto(
        string Diagnosis,
        string? Notes,
        List<MedicineItemDto> Medicines
    );
}
