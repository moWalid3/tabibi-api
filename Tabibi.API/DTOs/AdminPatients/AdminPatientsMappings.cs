using Tabibi.API.Common.Sorting;
using Tabibi.API.DTOs.Cities;
using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.AdminPatients
{
    public static class AdminPatientsMappings
    {
        public static readonly SortMappingDefinition<PatientDto, Patient> SortMapping = new()
        {
            Mappings = [
                new SortMapping(nameof(PatientDto.Name), nameof(Patient.Name)),
                new SortMapping(nameof(PatientDto.Email), nameof(Patient.Email)),
                new SortMapping(nameof(PatientDto.Gender), nameof(Patient.Gender)),
                new SortMapping(nameof(PatientDto.DateOfBirth), nameof(Patient.DateOfBirth)),
                new SortMapping(nameof(PatientDto.CreatedAtUtc), nameof(Patient.CreatedAtUtc)),
                new SortMapping(nameof(PatientDto.UpdatedAtUtc), nameof(Patient.UpdatedAtUtc)),
            ]
        };

        public static PatientDto ToDto(this Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Email = patient.Email!,
                AvatarUrl = patient.AvatarUrl,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                CreatedAtUtc = patient.CreatedAtUtc,
                UpdatedAtUtc = patient.UpdatedAtUtc,
                City = patient.City == null ? null : new CityDto
                {
                    Id = patient.City!.Id,
                    Name = patient.City.Name
                }
            };
        }
    }
}
