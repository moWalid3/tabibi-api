using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Patients
{
    public static class PatientMappings
    {
        public static PatientDto ToPatientDto(this ApplicationUser appUser)
        {
            return new PatientDto
            {
                Id = appUser.Id,
                Name = appUser.Name,
                Email = appUser.Email!,
                AvatarUrl = appUser.AvatarUrl,
                Gender = appUser.Gender,
                DateOfBirth = appUser.DateOfBirth,
                CreatedAtUtc = appUser.CreatedAtUtc,
                UpdatedAtUtc = appUser.UpdatedAtUtc
            };
        }
    }
}
