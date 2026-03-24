using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// Request to create a contribution (CC-01, CC-10)
/// </summary>
public record CreateContributionRequest(
    Guid GroupId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string BankAccountNumber
);

/// <summary>
/// Request to set up recurring debit order (CC-02)
/// </summary>
public record SetupRecurringContributionRequest(
    Guid GroupId,
    string BankAccountNumber,
    int DayOfMonth // Day of month to deduct (1-28)
);

/// <summary>
/// Response containing contribution details (CC-06, CC-09)
/// </summary>
public record ContributionResponse(
    Guid Id,
    Guid GroupId,
    string GroupName,
    Guid MemberId,
    string MemberName,
    decimal Amount,
    DateTime Date,
    ContributionStatus Status,
    PaymentMethod PaymentMethod,
    string TransactionId,
    DateTime? ConfirmedAt,
    string? FailureReason
);

/// <summary>
/// Contribution receipt details (CC-06)
/// </summary>
public record ContributionReceiptResponse(
    Guid Id,
    string GroupName,
    string MemberName,
    decimal Amount,
    DateTime Date,
    string TransactionId,
    decimal GroupBalanceAfter,
    decimal InterestEarnedToDate,
    string ReceiptNumber,
    ContributionStatus Status
);

/// <summary>
/// Request to retry failed contribution
/// </summary>
public record RetryContributionRequest(
    Guid ContributionId
);
