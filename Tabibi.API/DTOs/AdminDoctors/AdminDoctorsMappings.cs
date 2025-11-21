using Tabibi.API.Common.Sorting;
using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.AdminDoctors
{
    public static class AdminDoctorsMappings
    {
        public static readonly SortMappingDefinition<AdminDoctorDto, Doctor> SortMapping = new()
        {
            Mappings = [
                new SortMapping(nameof(AdminDoctorDto.Name), nameof(Doctor.Name)),
                new SortMapping(nameof(AdminDoctorDto.Email), nameof(Doctor.Email)),
                new SortMapping(nameof(AdminDoctorDto.Gender), nameof(Doctor.Gender)),
                new SortMapping(nameof(AdminDoctorDto.Status), nameof(Doctor.Status)),
                new SortMapping(nameof(AdminDoctorDto.ConsultationFee), nameof(Doctor.ConsultationFee)),
                new SortMapping(nameof(AdminDoctorDto.YearsOfExperience), nameof(Doctor.YearsOfExperience)),
                new SortMapping(nameof(Doctor.Department),
                    $"{nameof(Doctor.Department)}.{nameof(Doctor.Department.Name)}"),
                new SortMapping(nameof(Doctor.Clinic.City),
                    $"{nameof(Doctor.Clinic)}.{nameof(Doctor.Clinic.City)}.{nameof(Doctor.Clinic.City.Name)}"),
                new SortMapping(nameof(AdminDoctorDto.DateOfBirth), nameof(Doctor.DateOfBirth)),
                new SortMapping(nameof(AdminDoctorDto.CreatedAtUtc), nameof(Doctor.CreatedAtUtc)),
                new SortMapping(nameof(AdminDoctorDto.UpdatedAtUtc), nameof(Doctor.UpdatedAtUtc)),
            ]
        };

        public static AdminDoctorDto ToDto(this Doctor doctor)
        {
            return new AdminDoctorDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Email = doctor.Email!,
                AvatarUrl = doctor.AvatarUrl,
                Gender = doctor.Gender,
                Status = doctor.Status.ToString(),
                DateOfBirth = doctor.DateOfBirth,
                ConsultationFee = doctor.ConsultationFee,
                YearsOfExperience = doctor.YearsOfExperience,
                City = doctor.Clinic?.City?.Name,
                Department = doctor.Department?.Name,
                CreatedAtUtc = doctor.CreatedAtUtc,
                UpdatedAtUtc = doctor.UpdatedAtUtc
            };
        }
    }
}
