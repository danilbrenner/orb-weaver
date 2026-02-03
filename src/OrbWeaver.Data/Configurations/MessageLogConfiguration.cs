using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrbWeaver.Data.DataModel;

namespace OrbWeaver.Data.Configurations;

public class MessageLogConfiguration : IEntityTypeConfiguration<MessageLog>
{
    public void Configure(EntityTypeBuilder<MessageLog> builder)
    {
        builder.ToTable("messages_log");
        builder.HasKey(ml => ml.Hash);
        builder.Property(ml => ml.Hash).IsRequired().HasMaxLength(64);
        builder.Property(ml => ml.Payload).IsRequired().HasMaxLength(10000);
        builder.Property(ml => ml.LoggedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(ml => ml.Timestamp).IsRequired().HasDefaultValueSql("now()");
        
        builder.HasIndex(ml => ml.Payload).HasMethod("gin");
        builder.HasIndex(ml => ml.Timestamp);
    }
}

