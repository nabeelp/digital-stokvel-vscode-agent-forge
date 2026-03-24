using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// Request to create a new stokvel group (GM-01, GM-02, GM-06)
/// </summary>
public record CreateGroupRequest(
    string Name,
    string Description,
    GroupType GroupType,
    decimal ContributionAmount,
    ContributionFrequency ContributionFrequency,
    PayoutSchedule PayoutSchedule
);

/// <summary>
/// Request to update group details (GM-08)
/// </summary>
public record UpdateGroupRequest(
    string? Description,
    decimal? ContributionAmount,
    ContributionFrequency? ContributionFrequency
);

/// <summary>
/// Response containing group summary information
/// </summary>
public record GroupResponse(
    Guid Id,
    string Name,
    string Description,
    GroupType GroupType,
    decimal ContributionAmount,
    ContributionFrequency ContributionFrequency,
    PayoutSchedule PayoutSchedule,
    decimal CurrentBalance,
    decimal TotalInterestEarned,
    GroupStatus Status,
    int MemberCount,
    DateTime CreatedAt
);

/// <summary>
/// Detailed group response with members and stats (GM-07)
/// </summary>
public record GroupDetailsResponse(
    Guid Id,
    string Name,
    string Description,
    GroupType GroupType,
    decimal ContributionAmount,
    ContributionFrequency ContributionFrequency,
    PayoutSchedule PayoutSchedule,
    decimal CurrentBalance,
    decimal TotalInterestEarned,
    GroupStatus Status,
    DateTime CreatedAt,
    List<MemberSummary> Members,
    GroupStats Stats
);

/// <summary>
/// Member summary for group details
/// </summary>
public record MemberSummary(
    Guid Id,
    string FullName,
    string PhoneNumber,
    MemberRole Role,
    MemberStatus Status,
    decimal TotalContributed,
    DateTime JoinedAt,
    string ContributionStatus // "Current", "Late", "Delinquent"
);

/// <summary>
/// Group statistics
/// </summary>
public record GroupStats(
    decimal TotalContributions,
    decimal TotalPayouts,
    decimal TotalInterestEarned,
    int TotalMembers,
    int ActiveMembers,
    DateTime? NextPayoutDate
);

/// <summary>
/// Request to invite members to a group (GM-03, GM-04)
/// </summary>
public record InviteMemberRequest(
    string PhoneNumber,
    string FullName,
    MemberRole Role = MemberRole.Member
);

/// <summary>
/// Request to remove a member from a group (GM-05)
/// </summary>
public record RemoveMemberRequest(
    string Reason
);
