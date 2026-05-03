namespace Tabibi.API.DTOs.MedicalProfile
{
    public sealed record UpdateMedicalProfileDto(
        List<string>? ChronicDiseases,
        string? Medications,
        string? Allergies,
        List<string>? Surgeries,
        double? Weight,
        double? Height
    );
}
