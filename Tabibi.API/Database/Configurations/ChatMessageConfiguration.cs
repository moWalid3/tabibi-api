using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabibi.API.Entities;

namespace Tabibi.API.Database.Configurations
{
    public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Message).IsRequired().HasMaxLength(1000);
            builder.Property(m => m.SenderId).IsRequired();
            builder.Property(m => m.ReceiverId).IsRequired();

            builder.HasIndex(m => m.SenderId);
            builder.HasIndex(m => m.ReceiverId);
        }
    }
}
