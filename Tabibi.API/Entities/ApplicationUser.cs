using Microsoft.AspNetCore.Identity;

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
    }
}
