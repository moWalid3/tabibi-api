using Tabibi.API.DTOs.Cities;
using Tabibi.API.Entities;


namespace Tabibi.API.DTOs.Patients
{
    public static class PatientMappings
    {
        public static PatientProfileDto ToProfileDto(this Patient patient)
        {
            return new PatientProfileDto
            {
                Name = patient.Name,
                Email = patient.Email!,
                AvatarUrl = patient.AvatarUrl,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                City = patient.City == null ? null : new CityDto
                {
                    Id = patient.City!.Id,
                    Name = patient.City!.Name
                },
                CreatedAtUtc = patient.CreatedAtUtc,
                UpdatedAtUtc = patient.UpdatedAtUtc
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
