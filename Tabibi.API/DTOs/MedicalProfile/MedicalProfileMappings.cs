namespace Tabibi.API.DTOs.MedicalProfile
{
    public static class MedicalProfileMappings
    {
        public static MedicalProfileDto ToDto(this Entities.MedicalProfile profile)
        {
            return new MedicalProfileDto(
                profile.PatientId,
                profile.ChronicDiseases,
                profile.Medications,
                profile.Allergies,
                profile.Surgeries,
                profile.Weight,
                profile.Height,
                profile.IsCompleted,
                profile.UpdatedAt
            );
        }
    }
}
