using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrbWeaver.Infrastructure.DataModel;

namespace OrbWeaver.Infrastructure.Configurations;

public class AlertDataConfiguration : IEntityTypeConfiguration<AlertData>
{
    public void Configure(EntityTypeBuilder<AlertData> builder)
    {
        builder.ToTable("alerts").HasKey(ad => ad.AlertId);
        builder.Property(ad => ad.Name).IsRequired().HasMaxLength(256);
        builder.Property(ad => ad.Filter).IsRequired().HasColumnType("jsonpath");
        builder.Property(ad => ad.Expression).IsRequired().HasColumnType("jsonpath");
        builder.Property(ad => ad.LastHandledTs).IsRequired().HasDefaultValueSql("now()");
        builder.Property(ad => ad.Status).IsRequired().HasDefaultValue(0);
        builder
            .Property(ad => ad.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsConcurrencyToken()
            .IsConcurrencyToken();
    }
}