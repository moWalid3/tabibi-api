using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Diagnosis).HasMaxLength(500);
            builder.Property(p => p.Notes).HasMaxLength(1000);

            builder.HasOne(p => p.Booking)
                   .WithOne(b => b.Prescription)
                   .HasForeignKey<Prescription>(p => p.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
