namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// POPIA compliance service interface (SP-07, SP-13, SP-14)
/// </summary>
public interface IComplianceService
{
    string MaskPhoneNumber(string phoneNumber);
    string MaskIdNumber(string idNumber);
    Task<byte[]> ExportUserDataAsync(string userId);
    Task<bool> DeleteUserDataAsync(string userId);
    Task AuditDataAccessAsync(string userId, string accessedBy, string purpose);
    bool ShouldMaskPii(string requestingUserId, string targetUserId, string userRole);
}
