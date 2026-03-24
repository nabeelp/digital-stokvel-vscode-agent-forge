using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// Request to create group constitution (GG-01)
/// </summary>
public record CreateConstitutionRequest(
    Guid GroupId,
    string MissedPaymentPolicy,
    decimal? LateFeeAmount,
    int QuorumPercentage,
    string MemberRemovalProcess,
    string OtherRules
);

/// <summary>
/// Constitution response
/// </summary>
public record ConstitutionResponse(
    Guid Id,
    Guid GroupId,
    string MissedPaymentPolicy,
    decimal? LateFeeAmount,
    int QuorumPercentage,
    string MemberRemovalProcess,
    string OtherRules,
    DateTime CreatedAt,
    Guid CreatedByMemberId
);

/// <summary>
/// Request to create a vote proposal (GG-02, GG-03)
/// </summary>
public record CreateVoteRequest(
    Guid GroupId,
    string Proposal,
    string Description,
    DateTime Deadline,
    int QuorumPercentage
);

/// <summary>
/// Request to cast a vote (GG-04)
/// </summary>
public record CastVoteRequest(
    VoteChoice Choice,
    string? Comments
);

/// <summary>
/// Response containing vote details and results
/// </summary>
public record VoteResponse(
    Guid Id,
    Guid GroupId,
    string GroupName,
    string Proposal,
    string Description,
    DateTime Deadline,
    VoteStatus Status,
    int QuorumPercentage,
    VoteResults Results,
    DateTime CreatedAt,
    Guid CreatedByMemberId,
    string CreatedByName,
    bool HasVoted
);

/// <summary>
/// Vote results breakdown
/// </summary>
public record VoteResults(
    int YesCount,
    int NoCount,
    int AbstainCount,
    int TotalVotes,
    int TotalMembers,
    int QuorumRequired,
    bool QuorumMet,
    string Outcome // "Passed", "Failed", "Pending"
);

/// <summary>
/// Request to raise a dispute (GG-06)
/// </summary>
public record RaiseDisputeRequest(
    Guid GroupId,
    string IssueType,
    string Description
);

/// <summary>
/// Response containing dispute details
/// </summary>
public record DisputeResponse(
    Guid Id,
    Guid GroupId,
    string GroupName,
    Guid RaisedByMemberId,
    string RaisedByName,
    string IssueType,
    string Description,
    DisputeStatus Status,
    DateTime RaisedAt,
    DateTime? ResolvedAt,
    string? Resolution,
    bool IsEscalated,
    DateTime? EscalationDate,
    List<DisputeMessageResponse> Messages
);

/// <summary>
/// Dispute message in conversation
/// </summary>
public record DisputeMessageResponse(
    Guid Id,
    Guid MemberId,
    string MemberName,
    string Message,
    DateTime CreatedAt
);

/// <summary>
/// Request to add message to dispute (GG-07)
/// </summary>
public record AddDisputeMessageRequest(
    string Message
);

/// <summary>
/// Request to resolve dispute (GG-07)
/// </summary>
public record ResolveDisputeRequest(
    string Resolution
);
