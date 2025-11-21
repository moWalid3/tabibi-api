using Tabibi.API.DTOs.Cities;
using Tabibi.API.DTOs.Clinic;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.DTOs.WorkSchedule;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.DTOs.Doctors
{
    public static class DoctorMappings
    {
        public static DoctorProfileDto ToDoctorProfileDto(this Doctor doctor)
        {
            return new DoctorProfileDto
            {
                Name = doctor.Name,
                Email = doctor.Email!,
                AvatarUrl = doctor.AvatarUrl,
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                Bio = doctor.Bio,
                ConsultationFee = doctor.ConsultationFee,
                YearsOfExperience = doctor.YearsOfExperience,
                Department = doctor.Department == null ? null : new DepartmentBasicDto
                {
                    Id = doctor.Department!.Id,
                    Name = doctor.Department.Name,
                },
                CreatedAtUtc = doctor.CreatedAtUtc,
                UpdatedAtUtc = doctor.UpdatedAtUtc,
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

        public static void UpdateFromDto(this Doctor doctor, UpdateDoctorProfileDto dto)
        {
            doctor.Status = DoctorStatus.Pending;
            doctor.Name = dto.Name;
            doctor.AvatarUrl = dto.AvatarUrl;
            doctor.Gender = dto.Gender;
            doctor.DateOfBirth = dto.DateOfBirth;
            doctor.Bio = dto.Bio;
            doctor.ConsultationFee = dto.ConsultationFee;
            doctor.CredentialImageUrl = dto.CredentialImageUrl;
            doctor.YearsOfExperience = dto.YearsOfExperience;
            doctor.DepartmentId = dto.DepartmentId == null ? null : Guid.Parse(dto.DepartmentId);
            doctor.UpdatedAtUtc = DateTime.UtcNow;
        }

        public static void UpdateFromDto(this Entities.Clinic clinic, ClinicDetailsDto dto)
        {
            clinic.Name = dto.Name;
            clinic.Description = dto.Description;
            clinic.Address = dto.Address;
            clinic.ImageUrl = dto.ImageUrl;
            clinic.PhoneNumber = dto.PhoneNumber;
            clinic.Latitude = dto.Latitude;
            clinic.Longitude = dto.Longitude;
            clinic.CityId = Guid.Parse(dto.CityId);
            clinic.Longitude = dto.Longitude;
        }

        public static Entities.WorkSchedule ToEntity(this WorkScheduleDto dto, string doctorId)
        {
            return new Entities.WorkSchedule
            {
                Id = Guid.CreateVersion7(),
                DayOfWeek = dto.DayOfWeek,
                OpenTime = dto.OpenTime,
                CloseTime = dto.CloseTime,
                ClinicId = doctorId
            };
        }
    }
}
