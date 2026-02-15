using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.HasKey(f => new { f.PatientId, f.DoctorId });

            builder.Property(c => c.PatientId).HasMaxLength(450);
            builder.Property(c => c.DoctorId).HasMaxLength(450);

            builder.HasOne(f => f.Patient)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Doctor)
                .WithMany(d => d.Favorites)
                .HasForeignKey(f => f.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
