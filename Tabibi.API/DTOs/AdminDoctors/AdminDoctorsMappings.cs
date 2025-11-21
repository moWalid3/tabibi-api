using Tabibi.API.Common.Sorting;
using Tabibi.API.DTOs.Cities;
using Tabibi.API.DTOs.Clinic;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.DTOs.WorkSchedule;
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
                StatusCode = doctor.Status,
                DateOfBirth = doctor.DateOfBirth,
                ConsultationFee = doctor.ConsultationFee,
                YearsOfExperience = doctor.YearsOfExperience,
                City = doctor.Clinic?.City?.Name,
                Department = doctor.Department?.Name,
                CreatedAtUtc = doctor.CreatedAtUtc,
                UpdatedAtUtc = doctor.UpdatedAtUtc
            };
        }

        public static AdminDoctorDetailsDto ToAdminDoctorDetailsDto(this Doctor doctor)
        {
            return new AdminDoctorDetailsDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Email = doctor.Email!,
                Status = doctor.Status.ToString(),
                StatusCode = doctor.Status,
                AvatarUrl = doctor.AvatarUrl,
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                Bio = doctor.Bio,
                ConsultationFee = doctor.ConsultationFee,
                YearsOfExperience = doctor.YearsOfExperience,
                CredentialImageUrl = doctor.CredentialImageUrl,
                CreatedAtUtc = doctor.CreatedAtUtc,
                UpdatedAtUtc = doctor.UpdatedAtUtc,
                Department = doctor.Department == null ? null : new DepartmentBasicDto
                {
                    Id = doctor.Department!.Id,
                    Name = doctor.Department.Name
                },
                Clinic = doctor.Clinic == null ? null : new ClinicDto
                {
                    Name = doctor.Clinic!.Name,
                    Description = doctor.Clinic.Description,
                    Address = doctor.Clinic.Address,
                    ImageUrl = doctor.Clinic.ImageUrl,
                    Latitude = doctor.Clinic.Latitude,
                    Longitude = doctor.Clinic.Longitude,
                    PhoneNumber = doctor.Clinic.PhoneNumber,
                    City = new CityDto
                    {
                        Id = doctor.Clinic.City!.Id,
                        Name = doctor.Clinic.City.Name
                    }
                },
                Schedule = doctor.Clinic?.Schedule.Select(s => new WorkScheduleDto
                {
                    DayOfWeek = s.DayOfWeek,
                    OpenTime = s.OpenTime,
                    CloseTime = s.CloseTime
                }).ToList()
            };
        }
    }
}
