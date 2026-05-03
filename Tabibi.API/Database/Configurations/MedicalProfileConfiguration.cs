using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class MedicalProfileConfiguration : IEntityTypeConfiguration<MedicalProfile>
    {
        public void Configure(EntityTypeBuilder<MedicalProfile> builder)
        {
            builder.HasKey(m => m.PatientId);

            builder.HasOne(m => m.Patient)
                .WithOne(p => p.MedicalProfile)
                .HasForeignKey<MedicalProfile>(m => m.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Serialize Lists to JSON strings for database compatibility
            builder.Property(m => m.ChronicDiseases)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            builder.Property(m => m.Surgeries)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            builder.Property(m => m.Medications).HasMaxLength(1000);
            builder.Property(m => m.Allergies).HasMaxLength(1000);
        }
    }
}
