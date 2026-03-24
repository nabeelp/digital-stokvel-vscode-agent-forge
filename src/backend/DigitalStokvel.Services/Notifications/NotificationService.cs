using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Notifications;

/// <summary>
/// Placeholder notification service (implementation by notifications-engineer)
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task QueuePaymentReminderAsync(Guid memberId, Guid groupId, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Queueing payment reminder for member {MemberId} in group {GroupId}", memberId, groupId);
        await Task.CompletedTask;
    }

    public async Task QueuePaymentConfirmationAsync(Guid contributionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Queueing payment confirmation for contribution {ContributionId}", contributionId);
        await Task.CompletedTask;
    }

    public async Task QueuePayoutNotificationAsync(Guid payoutId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Queueing payout notification for payout {PayoutId}", payoutId);
        await Task.CompletedTask;
    }

    public async Task QueueInvitationNotificationAsync(Guid memberId, Guid groupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Queueing invitation notification for member {MemberId}", memberId);
        await Task.CompletedTask;
    }

    public async Task QueueDisputeNotificationAsync(Guid disputeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Queueing dispute notification for dispute {DisputeId}", disputeId);
        await Task.CompletedTask;
    }

    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // TODO: Implement SMS sending via Azure Communication Services or Twilio
        // For MVP, we'll just log the SMS
        _logger.LogInformation("SMS to {PhoneNumber}: {Message}", phoneNumber, message);
        await Task.CompletedTask;
    }
}
