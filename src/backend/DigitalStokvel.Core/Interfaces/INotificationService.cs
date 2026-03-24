namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for queuing notifications (placeholder for notifications-engineer)
/// </summary>
public interface INotificationService
{
    Task QueuePaymentReminderAsync(Guid memberId, Guid groupId, DateTime dueDate, CancellationToken cancellationToken = default);
    Task QueuePaymentConfirmationAsync(Guid contributionId, CancellationToken cancellationToken = default);
    Task QueuePayoutNotificationAsync(Guid payoutId, CancellationToken cancellationToken = default);
    Task QueueInvitationNotificationAsync(Guid memberId, Guid groupId, CancellationToken cancellationToken = default);
    Task QueueDisputeNotificationAsync(Guid disputeId, CancellationToken cancellationToken = default);
    
    // SMS notifications for authentication and security
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
