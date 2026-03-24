using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Group entity
/// </summary>
public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.GroupType)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.ContributionAmount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.ContributionFrequency)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.PayoutSchedule)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.CurrentBalance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.TotalInterestEarned)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(GroupStatus.Active);
        
        builder.Property(e => e.BankAccountNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(e => e.UpdatedAt);
        
        // Indexes
        builder.HasIndex(e => e.BankAccountNumber)
            .IsUnique();
        
        builder.HasIndex(e => e.ChairpersonId);
        
        builder.HasIndex(e => e.Status);
        
        // Relationships
        builder.HasMany(e => e.Members)
            .WithOne(m => m.Group)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.Contributions)
            .WithOne(c => c.Group)
            .HasForeignKey(c => c.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.Payouts)
            .WithOne(p => p.Group)
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.LedgerEntries)
            .WithOne(l => l.Group)
            .HasForeignKey(l => l.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.Votes)
            .WithOne(v => v.Group)
            .HasForeignKey(v => v.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.Disputes)
            .WithOne(d => d.Group)
            .HasForeignKey(d => d.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.InterestTransactions)
            .WithOne(i => i.Group)
            .HasForeignKey(i => i.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Constitution)
            .WithOne(c => c.Group)
            .HasForeignKey<GroupConstitution>(c => c.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
