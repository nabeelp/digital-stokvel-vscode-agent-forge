using DigitalStokvel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for LedgerEntry entity
/// IMPORTANT: Immutable ledger - no updates or deletes allowed (GW-06)
/// </summary>
public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.TransactionType)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.BalanceAfter)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.TransactionId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.Date)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Prevent updates - ledger is append-only
        builder.Property(e => e.UpdatedAt)
            .ValueGeneratedNever();
        
        // Indexes
        builder.HasIndex(e => new { e.GroupId, e.CreatedAt })
            .IsDescending(false, true); // Descending by CreatedAt for pagination
        
        builder.HasIndex(e => e.MemberId);
        
        builder.HasIndex(e => e.TransactionId);
        
        // Relationships
        builder.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
