using Tabibi.API.Common.Sorting;
using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.AdminPatients
{
    public static class AdminPatientsMappings
    {
        public static readonly SortMappingDefinition<AdminPatientDto, Patient> SortMapping = new()
        {
            Mappings = [
                new SortMapping(nameof(AdminPatientDto.Name), nameof(Patient.Name)),
                new SortMapping(nameof(AdminPatientDto.Email), nameof(Patient.Email)),
                new SortMapping(nameof(AdminPatientDto.Gender), nameof(Patient.Gender)),
                new SortMapping(nameof(AdminPatientDto.DateOfBirth), nameof(Patient.DateOfBirth)),
                new SortMapping(nameof(Patient.City),
                    $"{nameof(Patient.City)}.{nameof(Patient.City.Name)}"),
                new SortMapping(nameof(AdminPatientDto.CreatedAtUtc), nameof(Patient.CreatedAtUtc)),
                new SortMapping(nameof(AdminPatientDto.UpdatedAtUtc), nameof(Patient.UpdatedAtUtc)),
            ]
        };

        public static AdminPatientDto ToDto(this Patient patient)
        {
            return new AdminPatientDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Email = patient.Email!,
                AvatarUrl = patient.AvatarUrl,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                CreatedAtUtc = patient.CreatedAtUtc,
                UpdatedAtUtc = patient.UpdatedAtUtc,
                City = patient.City?.Name
            };
        }
    }
}
