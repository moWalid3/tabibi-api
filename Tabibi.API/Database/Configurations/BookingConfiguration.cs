using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.PricePaid).HasPrecision(18, 2);

            builder.Property(b => b.Status).HasConversion<int>();
            builder.Property(b => b.Type).HasConversion<int>();

            builder.HasIndex(b => new { b.Status, b.CreatedAt }); // for the background cleanup task

            builder.HasOne(b => b.Patient)
                   .WithMany()
                   .HasForeignKey(b => b.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Doctor)
                   .WithMany()
                   .HasForeignKey(b => b.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
