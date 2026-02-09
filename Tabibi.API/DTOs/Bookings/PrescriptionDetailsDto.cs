namespace Tabibi.API.DTOs.Bookings
{
    public sealed record PrescriptionDetailsDto(
        Guid Id,
        string Diagnosis,
        string? Notes,
        DateTime CreatedAt,
        List<MedicineItemDto> Medicines
    );
}
