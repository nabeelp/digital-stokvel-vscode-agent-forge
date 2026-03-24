namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Mock Core Banking API client for MVP (mocked per PRD Section 7.3)
/// </summary>
public interface ICoreBankingClient
{
    Task<string> CreateGroupSavingsAccountAsync(string groupName, string chairpersonUserId, CancellationToken cancellationToken = default);
    Task<bool> ExecutePaymentAsync(string fromAccount, string toAccount, decimal amount, string reference, CancellationToken cancellationToken = default);
    Task<bool> ExecuteEFTAsync(string toAccount, decimal amount, string reference, CancellationToken cancellationToken = default);
    Task<decimal> GetAccountBalanceAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<bool> SetupDebitOrderAsync(string accountNumber, decimal amount, int dayOfMonth, CancellationToken cancellationToken = default);
    Task<bool> CancelDebitOrderAsync(string accountNumber, string debitOrderId, CancellationToken cancellationToken = default);
}
