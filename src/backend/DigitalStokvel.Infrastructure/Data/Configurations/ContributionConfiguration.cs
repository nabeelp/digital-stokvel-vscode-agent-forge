using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Contribution entity
/// </summary>
public class ContributionConfiguration : IEntityTypeConfiguration<Contribution>
{
    public void Configure(EntityTypeBuilder<Contribution> builder)
    {
        builder.ToTable("Contributions");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.TransactionId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ContributionStatus.Pending);
        
        builder.Property(e => e.PaymentMethod)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.DueDate)
            .IsRequired();
        
        builder.Property(e => e.FailureReason)
            .HasMaxLength(500);
        
        builder.Property(e => e.RetryCount)
            .HasDefaultValue(0);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => new { e.GroupId, e.CreatedAt });
        
        builder.HasIndex(e => e.MemberId);
        
        builder.HasIndex(e => e.Status);
        
        builder.HasIndex(e => e.TransactionId)
            .IsUnique();
        
        builder.HasIndex(e => e.DueDate);
    }
}
