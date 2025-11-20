using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Doctors
{
    public sealed record GetDoctorStatusResponseDto(string Status, DoctorStatus StatusCode);
}
