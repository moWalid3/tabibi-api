using Microsoft.AspNetCore.Identity;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
        public Gender? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public required DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public string? FcmToken { get; set; }
    }
}
