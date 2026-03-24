namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Member notification preferences (ML-03)
/// </summary>
public class NotificationPreference : BaseEntity
{
    public Guid MemberId { get; set; }
    public bool EnablePushNotifications { get; set; } = true;
    public bool EnableSmsNotifications { get; set; } = true;
    public bool EnableContributionReminders { get; set; } = true;
    public bool EnablePayoutNotifications { get; set; } = true;
    public bool EnableVoteNotifications { get; set; } = true;
    public string PreferredLanguage { get; set; } = "en"; // en, zu, st, xh, af
    
    // Navigation properties
    public Member Member { get; set; } = null!;
}
