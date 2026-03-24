using DigitalStokvel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for GroupConstitution entity
/// </summary>
public class GroupConstitutionConfiguration : IEntityTypeConfiguration<GroupConstitution>
{
    public void Configure(EntityTypeBuilder<GroupConstitution> builder)
    {
        builder.ToTable("GroupConstitutions");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.MissedPaymentPolicy)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(e => e.LateFeeAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.QuorumThreshold)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0.50m); // 50% default
        
        builder.Property(e => e.MemberRemovalRules)
            .HasMaxLength(1000);
        
        builder.Property(e => e.GracePeriodDays)
            .HasDefaultValue(3);
        
        builder.Property(e => e.AllowPartialPayments)
            .HasDefaultValue(false);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => e.GroupId)
            .IsUnique(); // One constitution per group
    }
}
