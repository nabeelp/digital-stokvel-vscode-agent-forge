using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// Request to initiate a payout (PE-02, PE-05)
/// </summary>
public record InitiatePayoutRequest(
    Guid GroupId,
    Guid RecipientMemberId,
    decimal Amount,
    PayoutType PayoutType,
    string Notes
);

/// <summary>
/// Request to approve a payout (PE-03)
/// </summary>
public record ApprovePayoutRequest(
    string Notes
);

/// <summary>
/// Request to reject a payout
/// </summary>
public record RejectPayoutRequest(
    string Reason
);

/// <summary>
/// Response containing payout details (PE-09)
/// </summary>
public record PayoutResponse(
    Guid Id,
    Guid GroupId,
    string GroupName,
    Guid RecipientMemberId,
    string RecipientName,
    decimal Amount,
    decimal InterestIncluded,
    PayoutType PayoutType,
    PayoutStatus Status,
    DateTime InitiatedAt,
    Guid InitiatedByMemberId,
    string InitiatedByName,
    Guid? ApprovedByMemberId,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    DateTime? CompletedAt,
    DateTime? ExpiresAt,
    string TransactionId,
    string? FailureReason
);

/// <summary>
/// Payout history item
/// </summary>
public record PayoutHistoryItem(
    Guid Id,
    Guid RecipientMemberId,
    string RecipientName,
    decimal Amount,
    decimal InterestIncluded,
    PayoutType PayoutType,
    PayoutStatus Status,
    DateTime InitiatedAt,
    string InitiatedByName,
    string? ApprovedByName,
    DateTime? CompletedAt
);

/// <summary>
/// Calculate next payout recipient response (PE-01)
/// </summary>
public record NextPayoutRecipientResponse(
    Guid MemberId,
    string MemberName,
    string Reason,
    DateTime LastPayoutDate
);
