using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title).HasMaxLength(200).IsRequired();

            builder.Property(n => n.Message).HasMaxLength(500).IsRequired();

            builder.Property(n => n.UserId).IsRequired();
            
            builder.Property(n => n.Type).HasConversion<string>();
        }
    }
}
