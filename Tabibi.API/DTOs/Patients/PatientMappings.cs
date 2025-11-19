using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Patients
{
    public static class PatientMappings
    {
        public static PatientDto ToDto(this ApplicationUser appUser)
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

        public static PatientProfileDto ToProfileDto(this Patient patient)
        {
            return new PatientProfileDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Email = patient.Email!,
                AvatarUrl = patient.AvatarUrl,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                CityId = patient.CityId
            };
        }

        public static void UpdateFromDto(this Patient patient, UpdatePatientProfileDto dto)
        {
            patient.Name = dto.Name;
            patient.AvatarUrl = dto.AvatarUrl;
            patient.Gender = dto.Gender;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.CityId = dto.CityId == null ? null : Guid.Parse(dto.CityId);
            patient.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
