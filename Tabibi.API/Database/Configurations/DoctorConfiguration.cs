using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.Property(d => d.Bio).HasMaxLength(1000);

            builder.Property(d => d.ConsultationFee).HasColumnType("decimal(18,2)");
            builder.ToTable(t => t.HasCheckConstraint("CK_Doctor_ConsultationFee_Range", "ConsultationFee >= 0"));

            builder.Property(d => d.CredentialImageUrl).HasMaxLength(300);

            builder.HasOne(d => d.Clinic)
                .WithOne(c => c.Doctor)
                .HasForeignKey<Clinic>(c => c.Id);
        }
    }
}
