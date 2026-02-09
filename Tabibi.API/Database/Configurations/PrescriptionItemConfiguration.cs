using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
    {
        public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
        {
            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.MedicineName).IsRequired().HasMaxLength(200);
            builder.Property(pi => pi.Dosage).HasMaxLength(100);
            builder.Property(pi => pi.Frequency).HasMaxLength(100);
            builder.Property(pi => pi.Duration).HasMaxLength(100);
            builder.Property(pi => pi.Instructions).HasMaxLength(100);

            builder.HasOne(pi => pi.Prescription)
                   .WithMany(p => p.Medicines)
                   .HasForeignKey(pi => pi.PrescriptionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
