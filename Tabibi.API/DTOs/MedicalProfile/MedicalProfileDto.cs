namespace Tabibi.API.DTOs.MedicalProfile
{
    public sealed record MedicalProfileDto(
        string PatientId,
        List<string> ChronicDiseases,
        string? Medications,
        string? Allergies,
        List<string> Surgeries,
        double? Weight,
        double? Height,
        bool IsCompleted,
        DateTime UpdatedAt
    );
}
