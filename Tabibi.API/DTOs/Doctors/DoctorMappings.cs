using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Doctors
{
    public static class DoctorMappings
    {
        public static DoctorDto ToDoctorDto(this ApplicationUser appUser)
        {
            return new DoctorDto
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
