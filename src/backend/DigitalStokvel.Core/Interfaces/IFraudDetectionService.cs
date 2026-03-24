namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Fraud detection service interface
/// </summary>
public interface IFraudDetectionService
{
    Task<FraudCheckResult> DetectSuspiciousLoginAsync(string userId, string ipAddress, string deviceId);
    Task<FraudCheckResult> DetectSuspiciousContributionAsync(string contributionId);
    Task<FraudCheckResult> DetectSuspiciousPayoutAsync(string payoutId);
    Task FlagForReviewAsync(string entityId, string entityType, string reason);
}

/// <summary>
/// Fraud check result
/// </summary>
public class FraudCheckResult
{
    public bool IsSuspicious { get; set; }
    public int RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
}
