using DigitalStokvel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for InterestTransaction entity
/// </summary>
public class InterestTransactionConfiguration : IEntityTypeConfiguration<InterestTransaction>
{
    public void Configure(EntityTypeBuilder<InterestTransaction> builder)
    {
        builder.ToTable("InterestTransactions");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.InterestAmount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.AverageBalance)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.InterestRate)
            .HasPrecision(5, 4)
            .IsRequired();
        
        builder.Property(e => e.PeriodStart)
            .IsRequired();
        
        builder.Property(e => e.PeriodEnd)
            .IsRequired();
        
        builder.Property(e => e.DaysInPeriod)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => new { e.GroupId, e.PeriodEnd });
    }
}
