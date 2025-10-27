using Tabibi.API.DTOs.Auth;
using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Users
{
    public static class UserMappings
    {
        public static User ToEntity(this RegisterUserDto dto, string identityId)
        {
            return new User
            {
                Id = User.CreateNewId(),
                Name = dto.Name,
                Email = dto.Email,
                CreatedAtUtc = DateTime.UtcNow,
                IdentityId = identityId
            };
        }
    }
}
