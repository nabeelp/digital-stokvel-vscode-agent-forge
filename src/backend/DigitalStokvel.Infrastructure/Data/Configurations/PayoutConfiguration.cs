using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Payout entity
/// </summary>
public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("Payouts");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.InterestIncluded)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(PayoutStatus.Pending);
        
        builder.Property(e => e.PayoutType)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.TransactionId)
            .HasMaxLength(100);
        
        builder.Property(e => e.ApprovalExpiresAt)
            .IsRequired();
        
        builder.Property(e => e.FailureReason)
            .HasMaxLength(500);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => e.GroupId);
        
        builder.HasIndex(e => e.RecipientMemberId);
        
        builder.HasIndex(e => e.Status);
        
        builder.HasIndex(e => e.TransactionId);
        
        builder.HasIndex(e => e.ApprovalExpiresAt);
    }
}
