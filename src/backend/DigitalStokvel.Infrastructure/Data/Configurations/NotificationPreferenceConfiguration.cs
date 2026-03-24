using DigitalStokvel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalStokvel.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for NotificationPreference entity
/// </summary>
public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EnablePushNotifications)
            .HasDefaultValue(true);
        
        builder.Property(e => e.EnableSmsNotifications)
            .HasDefaultValue(true);
        
        builder.Property(e => e.EnableContributionReminders)
            .HasDefaultValue(true);
        
        builder.Property(e => e.EnablePayoutNotifications)
            .HasDefaultValue(true);
        
        builder.Property(e => e.EnableVoteNotifications)
            .HasDefaultValue(true);
        
        builder.Property(e => e.PreferredLanguage)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue("en");
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Indexes
        builder.HasIndex(e => e.MemberId)
            .IsUnique(); // One preference per member
    }
}
