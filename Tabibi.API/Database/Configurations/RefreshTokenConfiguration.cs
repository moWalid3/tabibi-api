using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Token).HasMaxLength(1000);
            builder.Property(rt => rt.UserId).HasMaxLength(450);

            builder.HasIndex(rt => rt.Token).IsUnique();

            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);
        }
    }
}
