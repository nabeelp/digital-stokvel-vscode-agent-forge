using DigitalStokvel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for AuditLog entity
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.EntityId)
            .IsRequired();
        
        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.BeforeState)
            .HasColumnType("jsonb"); // PostgreSQL JSONB type
        
        builder.Property(e => e.AfterState)
            .HasColumnType("jsonb"); // PostgreSQL JSONB type
        
        builder.Property(e => e.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(e => e.IpAddress)
            .HasMaxLength(45); // IPv6 max length
        
        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);
        
        // Indexes
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp });
        
        builder.HasIndex(e => e.UserId);
        
        builder.HasIndex(e => e.Timestamp);
    }
}
