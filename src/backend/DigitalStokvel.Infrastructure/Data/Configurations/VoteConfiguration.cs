using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Vote entity
/// </summary>
public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("Votes");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Proposal)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(e => e.Description)
            .HasMaxLength(2000);
        
        builder.Property(e => e.VoteDeadline)
            .IsRequired();
        
        builder.Property(e => e.Deadline)
            .IsRequired();
        
        builder.Property(e => e.QuorumThreshold)
            .HasPrecision(5, 2)
            .IsRequired();
        
        builder.Property(e => e.QuorumPercentage)
            .HasDefaultValue(50);
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(VoteStatus.Active);
        
        builder.Property(e => e.YesCount)
            .HasDefaultValue(0);
        
        builder.Property(e => e.NoCount)
            .HasDefaultValue(0);
        
        builder.Property(e => e.AbstainCount)
            .HasDefaultValue(0);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => new { e.GroupId, e.Status });
        
        builder.HasIndex(e => e.VoteDeadline);
        
        // Relationships
        builder.HasMany(e => e.VoteRecords)
            .WithOne(v => v.Vote)
            .HasForeignKey(v => v.VoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
