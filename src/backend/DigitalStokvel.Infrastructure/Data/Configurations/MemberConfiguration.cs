using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Member entity
/// </summary>
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.IdNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(MemberRole.Member);
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(MemberStatus.Invited);
        
        builder.Property(e => e.JoinedAt)
            .IsRequired();
        
        builder.Property(e => e.BankAccountNumber)
            .HasMaxLength(20);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => new { e.GroupId, e.UserId })
            .IsUnique();
        
        builder.HasIndex(e => new { e.GroupId, e.Status });
        
        builder.HasIndex(e => e.UserId);
        
        // Relationships
        builder.HasMany(e => e.Contributions)
            .WithOne(c => c.Member)
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.VoteRecords)
            .WithOne(v => v.Member)
            .HasForeignKey(v => v.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.NotificationPreference)
            .WithOne(n => n.Member)
            .HasForeignKey<NotificationPreference>(n => n.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
