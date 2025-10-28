using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.AvatarUrl).HasMaxLength(500);
            builder.Property(u => u.Name).HasMaxLength(300);
        }
    }
}
