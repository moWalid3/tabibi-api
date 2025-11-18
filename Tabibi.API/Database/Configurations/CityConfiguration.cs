using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(100);

            builder.HasMany(city => city.Patients)
                .WithOne(p => p.City)
                .HasForeignKey(p => p.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasMany(city => city.Clinics)
                .WithOne(clinic => clinic.City)
                .HasForeignKey(clinic => clinic.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
