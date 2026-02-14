using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record TransactionItemDto(
        Guid Id,
        string PatientName,
        string? PatientAvatarUrl,
        string Date,
        decimal PricePaid,
        AppointmentType Type
    );
}
