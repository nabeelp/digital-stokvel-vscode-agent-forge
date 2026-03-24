using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Dispute entity
/// </summary>
public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.ToTable("Disputes");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.IssueType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(DisputeStatus.Open);
        
        builder.Property(e => e.RaisedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(e => e.EscalationDeadline)
            .IsRequired();
        
        builder.Property(e => e.Resolution)
            .HasMaxLength(2000);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => new { e.GroupId, e.Status });
        
        builder.HasIndex(e => e.EscalationDeadline);
        
        // Relationships
        builder.HasMany(e => e.Messages)
            .WithOne(m => m.Dispute)
            .HasForeignKey(m => m.DisputeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
