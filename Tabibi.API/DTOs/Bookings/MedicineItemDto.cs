namespace Tabibi.API.DTOs.Bookings
{
    public sealed record MedicineItemDto(
        string MedicineName,
        string Dosage,
        string Frequency,
        string Duration,
        string Instructions
    );
}
