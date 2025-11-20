using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasMaxLength(450);

            builder.Property(c => c.Name).HasMaxLength(200);
            builder.Property(c => c.Description).HasMaxLength(1000);
            builder.Property(c => c.Address).HasMaxLength(300);
            builder.Property(c => c.ImageUrl).HasMaxLength(300);
            builder.Property(c => c.PhoneNumber).HasMaxLength(100);

            builder.HasMany(c => c.Schedule)
                .WithOne(ws => ws.Clinic)
                .HasForeignKey(ws => ws.ClinicId);
        }
    }
}
