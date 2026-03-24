using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// Response containing member profile information
/// </summary>
public record MemberResponse(
    Guid Id,
    Guid GroupId,
    string UserId,
    string PhoneNumber,
    string FullName,
    MemberRole Role,
    MemberStatus Status,
    DateTime JoinedAt,
    string GroupName
);

/// <summary>
/// Request to update member profile
/// </summary>
public record UpdateMemberProfileRequest(
    string? FullName,
    string? PhoneNumber
);

/// <summary>
/// Request to accept group invitation
/// </summary>
public record AcceptInvitationRequest(
    Guid GroupId,
    string BankAccountNumber
);

/// <summary>
/// Member contribution history summary
/// </summary>
public record MemberContributionHistory(
    Guid MemberId,
    string FullName,
    decimal TotalContributed,
    int TotalContributions,
    DateTime? LastContributionDate,
    List<ContributionSummary> RecentContributions
);

/// <summary>
/// Contribution summary for history
/// </summary>
public record ContributionSummary(
    Guid Id,
    decimal Amount,
    DateTime Date,
    ContributionStatus Status,
    string TransactionId
);
