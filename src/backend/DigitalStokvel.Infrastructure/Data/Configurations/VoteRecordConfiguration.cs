using DigitalStokvel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for VoteRecord entity
/// </summary>
public class VoteRecordConfiguration : IEntityTypeConfiguration<VoteRecord>
{
    public void Configure(EntityTypeBuilder<VoteRecord> builder)
    {
        builder.ToTable("VoteRecords");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.VoteChoice)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.VotedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => new { e.VoteId, e.MemberId })
            .IsUnique(); // One vote per member per proposal
    }
}
