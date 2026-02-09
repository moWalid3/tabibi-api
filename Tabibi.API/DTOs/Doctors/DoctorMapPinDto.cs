namespace Tabibi.API.DTOs.Doctors
{
    public sealed record DoctorMapPinDto(
        string Id,
        string Name,
        string Department,
        string ClinicName,
        double Latitude,
        double Longitude,
        string? AvatarUrl,
        decimal ConsultationFee
    );
}
